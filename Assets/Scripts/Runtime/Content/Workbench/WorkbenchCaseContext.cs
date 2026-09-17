using System;
using System.Collections.Generic;
using CurioClerk.Content.Incidents;
using CurioClerk.Core.Workbench;

namespace CurioClerk.Content.Workbench
{
    public sealed class WorkbenchCaseContext
    {
        private static readonly LocalizedCopy IcePurpose = C(
            "Keep the lost things safe. Find what is freezing the office.",
            "맡아 둔 분실물을 지키고, 보관소를 얼리는 원인을 찾기");
        private static readonly LocalizedCopy RainPurpose = C(
            "Help the senior understand the umbrella's unfinished message.",
            "선임을 도와 우산이 전하려는 말을 끝까지 듣기");
        private static readonly LocalizedCopy ReplyPurpose = C(
            "Help the senior answer Soyeon and pass her kindness on.",
            "소연의 부탁에 답하고, 다음 손님에게 우산을 건네기");
        private static readonly IReadOnlyDictionary<string, WorkbenchCaseContext> Contexts =
            new Dictionary<string, WorkbenchCaseContext>(StringComparer.Ordinal)
            {
                ["ice-01-crack"] = new WorkbenchCaseContext(IcePurpose, C(
                    "It is your first night caring for lost things. The senior is checking their owners' records while frost spreads from an ice block toward the shelves.",
                    "당신은 오늘 처음 출근한 야간 보관원입니다. 선임이 주인들의 기록을 찾는 동안, 얼음 덩어리의 서리가 분실물 선반까지 번지고 있습니다.")),
                ["ice-02-spread"] = new WorkbenchCaseContext(IcePurpose, C(
                    "Sealing the crack protected the desk, but did not explain the cold. The trapped leaf taps; the brass base answers with the same rhythm.",
                    "틈을 막아 책상은 지켰지만 냉기의 원인은 아직 모릅니다. 얼음 속 낙엽이 두드리면 황동 받침에서도 같은 박자로 소리가 납니다.")),
                ["ice-03-tomorrow"] = new WorkbenchCaseContext(IcePurpose, C(
                    "A watch came out of the base. The leaf vanished from the ice and appeared under its glass, blocking a hand. You do not yet know how it moved.",
                    "받침에서 시계가 나왔습니다. 얼음에서 사라진 낙엽이 시계 유리 안에 나타나 바늘을 막고 있습니다. 어떻게 옮겨 왔는지는 아직 모릅니다.")),
                ["ice-04-frozen-seal"] = new WorkbenchCaseContext(IcePurpose, C(
                    "When the watch ran, frost gathered on its crescent seal. An umbrella on the shelf bears the same mark. Something inside may connect them.",
                    "시계가 돌자 뒷면 초승달 봉인에 서리가 모였습니다. 선반의 우산에도 같은 무늬가 있습니다. 안에 둘을 잇는 단서가 있을지 모릅니다.")),
                ["ice-05-thaw"] = new WorkbenchCaseContext(IcePurpose, C(
                    "The key from the watch grows cold near the umbrella. Its torn patch releases the same cold as the ice block. You have found the source.",
                    "시계에서 찾은 열쇠가 우산 가까이에서 차가워집니다. 우산의 찢어진 천에서 얼음과 같은 냉기가 샙니다. 마침내 근원을 찾았습니다.")),
                ["rain-01-voices"] = new WorkbenchCaseContext(RainPurpose, C(
                    "Repairing the umbrella stopped the cold. Now its raindrops say, 'You said you'd keep a place for me.' The senior thinks the voice is speaking to them.",
                    "우산을 고치자 냉기가 멎었습니다. 이번에는 빗방울이 '내 자리 하나는 남겨 둔다고 했잖아'라고 말합니다. 선임은 자신에게 하는 말 같다고 합니다.")),
                ["rain-02-names-under-water"] = new WorkbenchCaseContext(RainPurpose, C(
                    "You caught one speaking drop. It said, 'Tuesday, at the window.' A soaked name label appeared beneath it; the senior recognizes the voice but needs the name.",
                    "따로 담은 빗방울이 '화요일, 창가에서'라고 말했습니다. 그 아래 젖은 이름표가 떠올랐습니다. 선임은 익숙한 목소리의 주인을 확인하고 싶어 합니다.")),
                ["rain-03-unsent-letter"] = new WorkbenchCaseContext(RainPurpose, C(
                    "The label named Soyeon, who taught the senior this job and lent them the umbrella. It was never returned. A letter folded into a fish hid beneath her name.",
                    "이름표의 소연은 선임에게 일을 가르치고 첫 출근 날 우산을 빌려준 분입니다. 선임은 우산을 돌려드리지 못했습니다. 이름표 밑에서 편지로 접은 물고기를 찾았습니다.")),
                ["rain-04-dry-order"] = new WorkbenchCaseContext(ReplyPurpose, C(
                    "Soyeon asked that the umbrella be lent to the next person in need. She wants a reply in its pocket, to know someone still keeps the office open. The senior is ready to answer.",
                    "소연은 다음에 필요한 사람에게 우산을 빌려주고, 비를 피할 자리도 남겨 달라고 했습니다. 보관소를 계속 지키는지 알고 싶다며 우산 주머니에 답장을 부탁했습니다.")),
                ["rain-05-testimony"] = new WorkbenchCaseContext(ReplyPurpose, C(
                    "The senior's reply is written: 'The umbrella is safe. We'll keep a place by the window.' It must reach the umbrella's pocket dry before the umbrella is offered to the next visitor.",
                    "선임의 답장을 썼습니다. '우산은 잘 있어요. 창가의 자리도 남겨 둘게요.' 답장을 마른 주머니에 넣고, 다음 손님이 빌릴 수 있게 우산을 준비할 차례입니다."))
            };

        private WorkbenchCaseContext(LocalizedCopy purpose, LocalizedCopy recap)
        { Purpose = purpose; Recap = recap; }
        public LocalizedCopy Purpose { get; }
        public LocalizedCopy Recap { get; }
        public static WorkbenchCaseContext Find(string stageId) => Contexts.TryGetValue(stageId, out var context) ? context : null;
        public static LocalizedCopy AfterRelatedAction(WorkbenchSession session, string targetId)
        {
            switch (session.Puzzle.Id + "/" + targetId)
            {
                case "ice-02-spread/base-lid":
                    if (session.HasCompleted("warm-rim")) return C("The rim is free of ice. The lid is loose, with a shallow notch beneath its edge.",
                        "테두리의 얼음이 녹았습니다. 덮개가 느슨해졌고 가장자리 아래에 얕은 홈이 있습니다.");
                    break;
                case "ice-03-tomorrow/crown":
                    if (session.HasCompleted("free-leaf")) return C("The leaf is clear of the hands. Nothing catches now; the square winding socket turns freely.",
                        "낙엽이 바늘에서 빠졌습니다. 걸리는 곳이 없어 네모난 태엽 홈이 부드럽게 돌아갑니다.");
                    break;
                case "ice-04-frozen-seal/back-notch":
                    if (session.HasCompleted("lift-back")) return C("The spring holds the back cover open. The notch is clear of ice.",
                        "용수철이 뒷뚜껑을 열린 채로 받치고 있습니다. 홈에도 얼음이 남아 있지 않습니다.");
                    break;
                case "ice-05-thaw/cold-patch":
                    if (session.HasCompleted("straighten-rib")) return C("The straightened rib no longer pulls the cloth apart. The edges now meet, but cold still escapes between the loose stitches.",
                        "우산살을 펴자 천의 양 끝이 맞닿았습니다. 더 벌어지지는 않지만 풀린 바늘땀 사이로 냉기가 샙니다.");
                    break;
                case "rain-03-unsent-letter/folded-fin":
                    if (session.HasCompleted("soften-crease")) return C("The damp crease is flexible now. A narrow gap beneath the outer fin leads into the folded page.",
                        "접힌 선이 촉촉하고 유연해졌습니다. 바깥 지느러미 밑의 좁은 틈이 접힌 편지 안쪽으로 이어집니다.");
                    break;
            }
            return null;
        }
        private static LocalizedCopy C(string english, string korean) => new LocalizedCopy(english, korean);
    }
}
