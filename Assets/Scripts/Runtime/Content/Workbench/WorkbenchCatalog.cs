using System;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Content.Incidents;
using CurioClerk.Core.Workbench;

namespace CurioClerk.Content.Workbench
{
    public static class WorkbenchCatalog
    {
        public static IReadOnlyList<WorkbenchSceneDefinition> All { get; }
            = Array.AsReadOnly(new[]
            {
                IceCrack(), MissingLeaf(), JammedWatch(), FrozenSeal(), ColdUmbrella(),
                RememberedVoice(), NameInTheJar(), UnsentLetter(), KeptPromise(), APlaceOutOfTheRain()
            });

        public static WorkbenchSceneDefinition Find(string stageId)
            => All.FirstOrDefault(scene => scene.StageId == stageId);

        private static WorkbenchSceneDefinition IceCrack()
            => new WorkbenchSceneDefinition(
                "ice-01-crack", "unmelting-ice",
                C("Cold on the desk", "책상이 얼고 있어요"),
                C("Stop the cold leaking from the crack.", "얼음 틈에서 새는 냉기를 막으세요."),
                C("We keep lost things safe until their owners return. I'll check the records; could you stop that ice freezing our desk?",
                    "여긴 주인이 올 때까지 분실물을 돌보는 곳이에요. 전 주인을 찾아볼 테니, 책상을 얼리는 저 얼음부터 좀 봐주세요."),
                C("That's better. The frost has stopped spreading. But look at the leaf inside...",
                    "이제 서리가 더 번지진 않네요. 그런데 안쪽 낙엽 좀 보세요..."),
                C("The leaf taps against the ice. The brass base taps back.", "낙엽이 얼음을 두드립니다. 황동 받침에서도 같은 소리가 납니다."),
                new[]
                {
                    T("crack", .35f, .58f, "Leaking crack", "냉기가 새는 틈",
                        "A wet crack breathes cold onto the desk. Putty won't grip the wet edge.",
                        "젖은 틈에서 찬 기운이 새어 나옵니다. 물기가 있으면 메움제가 붙지 않겠습니다."),
                    T("leaf", .49f, .48f, "Trapped leaf", "안쪽 낙엽",
                        "The leaf is still moving. Heating the whole block could hurt it; fill only the crack.",
                        "낙엽이 아직 움직입니다. 얼음 전체를 달구기보다는 틈만 막는 게 좋겠습니다."),
                    T("base", .49f, .20f, "Brass base", "황동 받침",
                        "Frost creeps down the base from the crack above. Something faintly ticks inside.",
                        "위쪽 틈에서 나온 서리가 받침을 타고 내려옵니다. 안에서 작은 소리도 납니다.")
                },
                new[]
                {
                    U("dry-cloth", "Dry cloth", "마른 천", "Soaks up water without warming the ice.", "얼음을 데우지 않고 물기만 닦습니다."),
                    U("insulating-putty", "Insulating putty", "틈 메움제", "Closes a small gap once its edges are dry.", "가장자리를 말린 뒤 작은 틈을 메웁니다.")
                },
                new[]
                {
                    A("dry-crack", "dry-cloth", "crack", "The cloth takes up the water. You can see where the cold escapes.",
                        "물기가 천에 스며듭니다. 냉기가 새는 틈이 또렷해졌습니다.",
                        "Inspect the leaking crack, then dry its wet edges.", "냉기가 새는 틈을 살펴보고 가장자리의 물기를 닦으세요.", "repair", new[] { "crack" }),
                    A("seal-crack", "insulating-putty", "crack", "You press putty into the dry crack. Frost pulls back from the desk.",
                        "마른 틈에 메움제를 누르자 책상 위 서리가 걷힙니다.",
                        "Check the leaf before using heat. Dry the crack, then seal only that gap.",
                        "낙엽부터 살펴보세요. 얼음을 달구지 말고, 물기를 닦은 틈만 메우면 됩니다.", "frost", new[] { "crack", "leaf" }, new[] { "dry-crack" })
                });

        private static WorkbenchSceneDefinition MissingLeaf()
            => new WorkbenchSceneDefinition(
                "ice-02-spread", "unmelting-ice",
                C("Ticking in the base", "받침 안에서 나는 소리"),
                C("Open the frozen base and follow the ticking.", "얼어붙은 받침을 열어 소리의 정체를 찾으세요."),
                C("Every time the leaf taps, something answers inside the base. Let's open it gently and find out what's ticking.",
                    "낙엽이 두드릴 때마다 받침 안에서도 소리가 나요. 조심히 열어서 뭐가 들어 있는지 봐요."),
                C("A watch! And there's our leaf, caught behind the glass. How did it get down there?",
                    "회중시계네요! 사라진 낙엽이 유리 안에 끼어 있어요. 대체 어떻게 들어간 걸까요?"),
                C("The watch tries to tick, but the leaf catches its hand.", "시계가 움직이려 할 때마다 낙엽이 바늘에 걸립니다."),
                new[]
                {
                    T("frozen-rim", .64f, .29f, "Frozen rim", "얼어붙은 테두리",
                        "Only a thin ring of ice joins the brass to its lid. The ice block above is still solid.",
                        "얇은 얼음 한 겹이 황동 테두리와 덮개를 붙잡고 있습니다. 위쪽 얼음 덩어리는 단단합니다."),
                    T("base-lid", .49f, .15f, "Base lid", "받침 덮개",
                        "A shallow notch runs under the lid. Pulling against the ice makes the soft brass flex.",
                        "덮개 아래 얕은 홈이 있습니다. 얼음에 붙은 채 당기니 무른 황동이 휘려고 합니다."),
                    T("watch-chain", .50f, .34f, "Small chain", "작은 사슬",
                        "A watch chain lies deep in the opened base. Only its little end ring stands clear of the rim.",
                        "열린 받침 깊숙이 시계 사슬이 있습니다. 끝의 작은 고리만 테두리 위로 나와 있습니다.", "lift-base")
                },
                new[]
                {
                    U("warm-pad", "Warm pad", "따뜻한 찜질팩", "Softens a thin layer of ice on metal.", "금속에 붙은 얇은 얼음을 녹입니다."),
                    U("wooden-wedge", "Wooden wedge", "나무 쐐기", "Lifts a loose lid without scratching it.", "느슨해진 덮개를 흠집 없이 들어 올립니다."),
                    U("fine-tweezers", "Fine tweezers", "가는 핀셋", "Grips small rings and loose fragments.", "작은 고리나 떨어진 조각을 집습니다.")
                },
                new[]
                {
                    A("warm-rim", "warm-pad", "frozen-rim", "The ice ring melts. The base lid shifts under your hand.",
                        "테두리의 얼음이 녹습니다. 손끝에서 덮개가 조금 움직입니다.",
                        "Find the ice holding the brass rim, then warm just that part.", "황동 테두리를 붙잡은 얼음을 찾아 그 부분만 데우세요.", "warmth", new[] { "frozen-rim" }),
                    A("lift-base", "wooden-wedge", "base-lid", "The lid lifts. The leaf vanishes from the ice, and a tiny chain rattles below.",
                        "덮개가 들리자 얼음 속 낙엽이 사라집니다. 받침 안에서 작은 사슬이 달그락거립니다.",
                        "Melt the frozen rim before putting the wedge into the lid's notch.", "테두리 얼음을 녹인 뒤 덮개의 홈에 쐐기를 넣으세요.", "reveal", new[] { "base-lid" }, new[] { "warm-rim" }),
                    A("retrieve-watch", "fine-tweezers", "watch-chain", "You lift the chain. A mossy watch follows, with the missing leaf under its glass.",
                        "사슬을 들어 올리자 이끼 낀 시계가 따라 나옵니다. 유리 아래에 사라진 낙엽이 있습니다.",
                        "Open the base, inspect the little chain, and lift it by the ring.", "받침을 열고 작은 사슬을 살펴본 뒤 고리를 집어 올리세요.", "reveal", new[] { "watch-chain" }, new[] { "lift-base" }, "mossy-watch")
                });

        private static WorkbenchSceneDefinition JammedWatch()
            => new WorkbenchSceneDefinition(
                "ice-03-tomorrow", "mossy-watch",
                C("A leaf between the hands", "바늘에 걸린 낙엽"),
                C("Free the leaf and get the watch ticking again.", "낙엽을 빼내 시계가 다시 움직이게 하세요."),
                C("Don't wind it yet. The hand is catching on that leaf. We need to clear the little glass cover first.",
                    "아직 태엽은 감지 마세요. 바늘이 낙엽에 걸려 있어요. 작은 유리 덮개부터 열어야겠네요."),
                C("There it goes. But the back is freezing over now. Something inside really doesn't want us to open it.",
                    "이제 움직이네요. 그런데 이번엔 뒷면이 얼고 있어요. 안에 뭐가 더 들어 있나 봐요."),
                C("A crescent is engraved on the back seal. The same mark is sewn onto an umbrella nearby.",
                    "뒷면 봉인에 초승달이 새겨져 있습니다. 옆에 놓인 우산에도 같은 무늬가 있습니다."),
                new[]
                {
                    T("moss", .73f, .30f, "Mossy hinge", "이끼 낀 경첩",
                        "Loose moss tangles around the hinge. The glass cover moves a little, then catches on it.",
                        "느슨한 이끼가 경첩에 엉켰습니다. 유리 덮개가 조금 움직이다 이끼에 걸립니다."),
                    T("caught-leaf", .48f, .50f, "Caught leaf", "걸린 낙엽",
                        "The stem is wedged under a hand. One end sticks out, well clear of the tiny gears.",
                        "줄기가 시곗바늘 밑에 끼었습니다. 한쪽 끝은 작은 톱니들과 떨어진 채 튀어나와 있습니다.", "clear-hinge"),
                    T("crown", .29f, .87f, "Winding crown", "태엽 꼭지",
                        "The crown has a square socket. It resists turning whenever the hand presses against the leaf.",
                        "꼭지에 네모난 홈이 있습니다. 바늘이 낙엽을 누를 때마다 꼭지도 뻑뻑해집니다.")
                },
                new[]
                {
                    U("soft-brush", "Soft brush", "부드러운 솔", "Sweeps loose moss away from a hinge.", "경첩 주변의 느슨한 이끼를 털어냅니다."),
                    U("fine-tweezers", "Fine tweezers", "가는 핀셋", "Lifts a leaf by its stem.", "낙엽의 줄기를 잡아 들어 올립니다."),
                    U("winding-key", "Winding key", "태엽 열쇠", "Turns the crown once the mechanism is free.", "걸린 것이 없을 때 태엽 꼭지를 돌립니다.")
                },
                new[]
                {
                    A("clear-hinge", "soft-brush", "moss", "The moss brushes off. The glass cover springs open, exposing the trapped stem.",
                        "이끼가 털리자 유리 덮개가 열립니다. 끼어 있는 낙엽 줄기가 드러났습니다.",
                        "Inspect the mossy hinge and brush away the loose strands.", "이끼 낀 경첩을 살펴보고 느슨한 가닥을 털어내세요.", "reveal", new[] { "moss" }),
                    A("free-leaf", "fine-tweezers", "caught-leaf", "The leaf slides free in one piece. The hand settles into its groove.",
                        "낙엽이 찢어지지 않고 빠집니다. 들려 있던 바늘이 제자리에 내려앉습니다.",
                        "Open the cover, inspect the trapped stem, and lift its free end.", "덮개를 열고 끼어 있는 줄기를 살펴본 뒤 끝을 집어 올리세요.", "repair", new[] { "caught-leaf" }, new[] { "clear-hinge" }),
                    A("wind-watch", "winding-key", "crown", "The crown turns smoothly. Tick, tick: the watch runs, and frost forms around its back seal.",
                        "꼭지가 부드럽게 돌아갑니다. 째깍, 째깍. 시계가 움직이자 뒷면 봉인에 서리가 맺힙니다.",
                        "Remove the leaf before using the winding key on the crown.", "낙엽을 빼낸 다음 태엽 열쇠로 꼭지를 돌리세요.", "turn", new[] { "crown" }, new[] { "free-leaf" })
                });

        private static WorkbenchSceneDefinition FrozenSeal()
            => new WorkbenchSceneDefinition(
                "ice-04-frozen-seal", "mossy-watch",
                C("Under the frozen seal", "얼어붙은 봉인 아래"),
                C("Open the watch's back without damaging the object inside.", "안에 든 물건을 망가뜨리지 않고 시계 뒷면을 여세요."),
                C("The crescent seal is holding the back shut. Warm the seal gently; I'll keep the watch steady.",
                    "초승달 봉인이 뒷뚜껑을 붙잡고 있어요. 봉인만 살짝 데워 주세요. 시계는 제가 잡고 있을게요."),
                C("This key has the same crescent. It must belong to that locked umbrella strap.",
                    "열쇠에도 초승달이 있네요. 저 우산의 잠긴 끈에 맞을 것 같아요."),
                C("The key grows cold when pointed at the umbrella. The ice, watch and umbrella are connected.",
                    "열쇠를 우산 쪽으로 향하면 차가워집니다. 얼음과 시계, 우산이 서로 연결돼 있습니다."),
                new[]
                {
                    T("back-seal", .34f, .51f, "Frozen seal", "얼어붙은 봉인",
                        "Ice covers only the crescent seal. The delicate gears underneath must not get too hot.",
                        "초승달 봉인에만 얼음이 맺혔습니다. 안쪽의 섬세한 톱니는 너무 달구면 안 되겠습니다."),
                    T("back-notch", .20f, .38f, "Lid notch", "뒷뚜껑 홈",
                        "The lid has a thin lip beside this notch. It strains against the frozen seal when lifted.",
                        "홈 옆에 뚜껑의 얇은 가장자리가 있습니다. 들어 올리면 얼어붙은 봉인이 버팁니다."),
                    T("hidden-key", .55f, .43f, "Crescent key", "초승달 열쇠",
                        "A small key rests beside the moving gears. Its crescent loop is the only part that is clear.",
                        "움직이는 톱니 옆에 작은 열쇠가 놓여 있습니다. 초승달 고리만 다른 부품에 닿지 않습니다.", "lift-back")
                },
                new[]
                {
                    U("warm-pad", "Warm pad", "따뜻한 찜질팩", "Thaws a frozen seal a little at a time.", "얼어붙은 봉인을 조금씩 녹입니다."),
                    U("thin-lever", "Thin lever", "얇은 주걱", "Gently lifts a loosened metal cover.", "느슨해진 금속 덮개를 살며시 들어 올립니다."),
                    U("fine-tweezers", "Fine tweezers", "가는 핀셋", "Reaches small objects beside the gears.", "톱니 옆의 작은 물건을 꺼냅니다.")
                },
                new[]
                {
                    A("thaw-seal", "warm-pad", "back-seal", "The seal sheds its ice. A thin gap appears around the back cover.",
                        "봉인 위 얼음이 녹습니다. 뒷뚜껑 둘레에 가느다란 틈이 생겼습니다.",
                        "Inspect the crescent seal and warm it before lifting the cover.", "초승달 봉인을 살펴보고 먼저 데운 뒤 뚜껑을 여세요.", "warmth", new[] { "back-seal" }),
                    A("lift-back", "thin-lever", "back-notch", "The back opens. A key trembles beside the ticking gears.",
                        "뒷뚜껑이 열립니다. 움직이는 톱니 옆에서 열쇠 하나가 떨고 있습니다.",
                        "Thaw the seal, then use the lever in the lid's notch.", "봉인을 녹인 다음 뒷뚜껑 홈에 주걱을 넣으세요.", "reveal", new[] { "back-notch" }, new[] { "thaw-seal" }),
                    A("take-key", "fine-tweezers", "hidden-key", "You lift the key safely out. Its crescent turns toward the umbrella.",
                        "열쇠를 무사히 꺼냅니다. 열쇠의 초승달이 우산 쪽을 향합니다.",
                        "Open the back, inspect the key, and pick it up by its loop.", "뒷뚜껑을 열고 열쇠를 살펴본 뒤 고리를 집으세요.", "reveal", new[] { "hidden-key" }, new[] { "lift-back" }, "whispering-key")
                });

        private static WorkbenchSceneDefinition ColdUmbrella()
            => new WorkbenchSceneDefinition(
                "ice-05-thaw", "moon-umbrella",
                C("The cold has a source", "냉기가 시작된 곳"),
                C("Release the bent rib and close the umbrella's cold leak.", "휘어진 우산살을 펴고 냉기가 새는 곳을 꿰매세요."),
                C("The cold is coming through this torn patch. The bent rib keeps pulling it open. Let's fix both.",
                    "찢어진 덧댐에서 냉기가 새요. 휘어진 우산살이 계속 천을 벌리고 있네요. 둘 다 고쳐야겠어요."),
                C("The room's warming up. You've saved the other things on the shelves. Wait... is that someone speaking inside the umbrella?",
                    "방이 따뜻해지고 있어요. 선반의 물건들도 이제 괜찮겠네요. 잠깐... 우산 안에서 누가 말하지 않았어요?"),
                C("The cold stops. A single raindrop repeats: 'You said you'd keep a place for me.'",
                    "냉기가 멎습니다. 빗방울 하나가 말을 되풀이합니다. '내 자리 하나는 남겨 둔다고 했잖아.'"),
                new[]
                {
                    T("locked-strap", .49f, .47f, "Locked strap", "잠긴 우산 끈",
                        "The strap's lock bears a crescent. We saw that same mark inside the watch.",
                        "끈의 자물쇠에 초승달이 있습니다. 시계 안에서 봤던 무늬입니다."),
                    T("bent-rib", .63f, .57f, "Bent rib", "휘어진 우산살",
                        "One bent rib presses into the torn patch. Its sharp edge rubs against the thin cloth.",
                        "휘어진 우산살 하나가 찢어진 천을 밀고 있습니다. 날카로운 끝이 얇은 천에 닿아 있습니다.", "unlock-strap"),
                    T("cold-patch", .38f, .31f, "Torn patch", "찢어진 덧댐",
                        "Cold pours between the loose stitches. The bent rib keeps pulling the torn edges apart.",
                        "풀린 바늘땀 사이로 냉기가 쏟아집니다. 휘어진 우산살이 찢어진 천을 계속 벌립니다.")
                },
                new[]
                {
                    U("crescent-key", "Crescent key", "초승달 열쇠", "The key found inside the pocket watch.", "회중시계 안에서 찾은 열쇠입니다."),
                    U("padded-pliers", "Padded pliers", "천을 감싼 펜치", "Straightens a rib without biting into fabric.", "천을 상하게 하지 않고 우산살을 폅니다."),
                    U("waxed-thread", "Needle and thread", "바늘과 실", "Closes the gap along a loose patch.", "덧댐의 벌어진 틈을 꿰맵니다.")
                },
                new[]
                {
                    A("unlock-strap", "crescent-key", "locked-strap", "The lock clicks. The strap loosens, exposing a bent rib beneath the cloth.",
                        "딸깍. 끈이 풀리며 천 아래 휘어진 우산살이 드러납니다.",
                        "Inspect the strap's keyhole and try the key from the watch.", "끈의 열쇠 구멍을 살펴보고 시계에서 찾은 열쇠를 써 보세요.", "turn", new[] { "locked-strap" }),
                    A("straighten-rib", "padded-pliers", "bent-rib", "The rib straightens. The torn edges fall together instead of pulling apart.",
                        "우산살이 펴집니다. 벌어지던 천의 양 끝이 나란히 맞닿습니다.",
                        "Unlock the strap, find the bent rib, and straighten it with padded pliers.", "끈을 풀고 휘어진 우산살을 찾아 펜치로 펴세요.", "repair", new[] { "bent-rib" }, new[] { "unlock-strap" }),
                    A("mend-cold-patch", "waxed-thread", "cold-patch", "The last stitch holds. Frost falls from the shelves, and the desk becomes warm to the touch.",
                        "마지막 바늘땀이 버팁니다. 선반의 서리가 떨어지고 책상에 온기가 돌아옵니다.",
                        "Straighten the rib so it stops tearing the patch, then stitch the gap shut.", "우산살을 펴서 천이 벌어지지 않게 한 다음 틈을 꿰매세요.", "warmth", new[] { "cold-patch" }, new[] { "straighten-rib" })
                });

        private static WorkbenchSceneDefinition RememberedVoice()
            => new WorkbenchSceneDefinition(
                "rain-01-voices", "moon-umbrella",
                C("A voice in one drop", "빗방울 속 목소리"),
                C("Catch the speaking drop without spilling it.", "말하는 빗방울을 흘리지 않고 병에 담으세요."),
                C("That voice sounds familiar. The drops keep talking over each other. Can we catch just the one by the seam?",
                    "어디서 들어 본 목소리예요. 빗방울들이 한꺼번에 말해서 잘 안 들리네요. 솔기 쪽 한 방울만 담아 볼까요?"),
                C("'Tuesday, at the window.' I know that voice. Let's look at the name floating in the jar.",
                    "'화요일, 창가에서.' 아는 목소리가 맞아요. 병에 이름표 같은 게 떠 있네요. 한번 봐요."),
                C("A waterlogged name label appears beneath the captured drop.", "담아 둔 빗방울 밑으로 젖은 이름표가 떠오릅니다."),
                new[]
                {
                    T("speaking-drop", .62f, .58f, "Speaking drop", "말하는 빗방울",
                        "One drop hangs from the seam. Its voice is clearer than the others, but it is about to fall.",
                        "솔기에 한 방울이 매달려 있습니다. 다른 방울보다 목소리가 또렷하지만 곧 떨어지겠습니다."),
                    T("handle", .75f, .84f, "Worn handle", "닳은 손잡이",
                        "The handle is worn smooth where someone held it. A faint voice says, 'Tuesday, at the window.'",
                        "오래 쥐었던 자리가 반들반들합니다. 희미하게 '화요일, 창가에서'라는 말이 들립니다."),
                    T("jar-mouth", .45f, .43f, "Filled jar", "빗방울을 담은 병",
                        "The captured drop circles toward the open mouth. The rain outside seems to be drawing it back.",
                        "담아 둔 빗방울이 열린 입구 쪽으로 맴돕니다. 바깥의 비가 다시 끌어당기는 것 같습니다.", "catch-drop")
                },
                new[]
                {
                    U("empty-jar", "Empty jar", "빈 유리병", "Catches one drop so it can be heard clearly.", "빗방울 하나를 담아 목소리를 따로 듣습니다."),
                    U("brass-lid", "Brass lid", "황동 뚜껑", "Keeps the captured drop inside its jar.", "담아 둔 빗방울이 병 밖으로 나오지 않게 합니다.")
                },
                new[]
                {
                    A("catch-drop", "empty-jar", "speaking-drop", "Plink. One drop lands in the jar. Its voice is suddenly clear.",
                        "톡. 빗방울 하나가 병에 들어갑니다. 목소리가 갑자기 또렷해졌습니다.",
                        "Inspect the drop hanging from the seam and hold the jar below it.", "솔기에 매달린 빗방울을 살펴본 뒤 아래에 병을 대세요.", "rain", new[] { "speaking-drop" }),
                    A("keep-voice", "brass-lid", "jar-mouth", "The lid holds the drop in place. A soaked label rises to the surface beneath it.",
                        "뚜껑이 빗방울을 붙잡습니다. 그 밑에서 젖은 이름표 하나가 떠오릅니다.",
                        "Catch the drop first, then inspect and close the filled jar.", "빗방울을 먼저 담으세요. 채워진 병을 살펴보고 뚜껑을 닫으면 됩니다.", "reveal", new[] { "jar-mouth" }, new[] { "catch-drop" }, "rain-jar")
                });

        private static WorkbenchSceneDefinition NameInTheJar()
            => new WorkbenchSceneDefinition(
                "rain-02-names-under-water", "rain-jar",
                C("A name under the water", "물에 잠긴 이름"),
                C("Lower the water and save the name on the label.", "물을 덜어 내고 이름표의 글씨를 살리세요."),
                C("Don't tip the jar; the ink will wash off. Take the cloudy water out a little at a time.",
                    "병을 쏟으면 글씨도 씻겨 나가겠어요. 탁한 물만 조금씩 덜어 내 주세요."),
                C("Soyeon... She taught me this job. She lent me that umbrella on my first night, and I never gave it back.",
                    "소연 선배... 제게 일을 가르쳐 준 분이에요. 첫날 저 우산을 빌려주셨는데, 결국 못 돌려드렸어요."),
                C("The name label lifts away to reveal a small fish folded from a letter.",
                    "이름표 아래에 편지로 접은 작은 물고기가 숨어 있었습니다."),
                new[]
                {
                    T("tight-lid", .49f, .80f, "Tight lid", "꽉 잠긴 뚜껑",
                        "Wet brass keeps slipping under your hand. The lid must turn while the glass stays still.",
                        "젖은 황동이 손에서 자꾸 미끄러집니다. 유리병은 가만히 두고 뚜껑만 돌려야겠습니다."),
                    T("cloudy-water", .50f, .27f, "Cloudy water", "탁한 물",
                        "Cloudy water covers the name. Tilting the jar would wash the loose ink off the label.",
                        "탁한 물이 이름을 덮었습니다. 병을 기울이면 느슨해진 잉크까지 씻겨 나가겠습니다."),
                    T("name-label", .37f, .35f, "Soaked label", "젖은 이름표",
                        "Letters show through a film of water. The ink shifts at even a light touch.",
                        "얇은 물기 아래로 글씨가 비칩니다. 살짝 건드리기만 해도 잉크가 움직입니다.", "lower-water")
                },
                new[]
                {
                    U("rubber-grip", "Rubber grip", "고무 받침", "Holds a slippery lid while it turns.", "미끄러운 뚜껑을 단단히 잡아 줍니다."),
                    U("glass-dropper", "Glass dropper", "유리 스포이트", "Draws out water without tipping the jar.", "병을 기울이지 않고 물만 덜어 냅니다."),
                    U("blotting-paper", "Blotting paper", "물기 흡수지", "Draws moisture away from fragile writing.", "번지기 쉬운 글씨 위의 물기를 빨아들입니다.")
                },
                new[]
                {
                    A("open-jar", "rubber-grip", "tight-lid", "The lid turns without a splash. The cloudy water settles.",
                        "물 한 방울 튀지 않고 뚜껑이 돌아갑니다. 탁한 물이 가라앉습니다.",
                        "Inspect the slippery lid and turn it with the rubber grip.", "미끄러운 뚜껑을 살펴보고 고무 받침으로 잡아 돌리세요.", "turn", new[] { "tight-lid" }),
                    A("lower-water", "glass-dropper", "cloudy-water", "The water level drops below the label. Its ink stays where it belongs.",
                        "물이 이름표 아래까지 줄어듭니다. 글씨는 씻겨 나가지 않고 남았습니다.",
                        "Open the lid before using the dropper on the cloudy water.", "뚜껑을 연 뒤 스포이트로 탁한 물을 덜어 내세요.", "rain", new[] { "cloudy-water" }, new[] { "open-jar" }),
                    A("save-name", "blotting-paper", "name-label", "The paper drinks up the water: 'Soyeon, Tuesday nights.' A folded fish flicks its tail underneath.",
                        "흡수지가 물기를 머금습니다. '소연, 화요일 야간.' 이름표 밑에서 종이 물고기가 꼬리를 흔듭니다.",
                        "Lower the water, inspect the exposed label, then blot it without rubbing.", "물을 덜어 내고 드러난 이름표를 살펴보세요. 문지르지 말고 흡수지를 대면 됩니다.", "reveal", new[] { "name-label" }, new[] { "lower-water" }, "paper-fish")
                });

        private static WorkbenchSceneDefinition UnsentLetter()
            => new WorkbenchSceneDefinition(
                "rain-03-unsent-letter", "paper-fish",
                C("A letter with fins", "지느러미 달린 편지"),
                C("Unfold the paper fish without tearing the letter.", "편지가 찢어지지 않게 종이 물고기를 펼치세요."),
                C("That's Soyeon's handwriting between the folds. The paper has gone stiff. Let's soften the crease before we open it.",
                    "접힌 틈에 소연 선배의 글씨가 보여요. 종이가 뻣뻣해졌네요. 접힌 자국부터 부드럽게 풀어 줘요."),
                C("'To the next night clerk: leave one dry place for anyone who comes in.' She left this for us.",
                    "'다음 야간 직원에게. 찾아오는 누구에게든 비를 피할 자리 하나는 남겨 주세요.' 우리에게 남긴 편지였어요."),
                C("The letter ends: 'Leave your reply in the voice box. I'd like to know this reached you.'",
                    "편지 끝에 적혀 있습니다. '답장은 웅얼거림 상자에 넣어 주세요. 이 편지가 잘 도착했는지 알고 싶어요.'"),
                new[]
                {
                    T("stiff-crease", .43f, .55f, "Stiff crease", "굳은 접힌 자국",
                        "The dry crease cracks when pulled. The writing stops short of this narrow, unmarked fold.",
                        "마른 접힌 자국을 당기면 갈라집니다. 글씨는 이 좁은 선까지 닿지 않았습니다."),
                    T("folded-fin", .48f, .28f, "Folded fin", "접힌 지느러미",
                        "The fin is the outermost fold. A narrow gap leads beneath it, but the crease still resists.",
                        "지느러미가 가장 바깥쪽에 접혔습니다. 아래로 얕은 틈이 이어지지만 접힌 선이 아직 뻣뻣합니다."),
                    T("letter-lines", .54f, .50f, "Written lines", "편지의 글씨",
                        "The opened sheet curls while wet. Tiny beads of ink begin to spread along the wrinkles.",
                        "펼친 종이가 젖은 채 말려 올라갑니다. 작은 잉크 방울이 주름을 따라 번지려 합니다.", "unfold-fish")
                },
                new[]
                {
                    U("water-brush", "Damp brush", "살짝 젖은 붓", "Softens a dry fold with very little water.", "적은 물로 굳은 접힌 자국을 풀어 줍니다."),
                    U("paper-folder", "Paper folder", "종이 접기 주걱", "Slides under a softened paper fold.", "부드러워진 종이 틈을 벌려 줍니다."),
                    U("blotting-paper", "Blotting paper", "물기 흡수지", "Presses a damp page flat without smearing ink.", "잉크를 번지게 하지 않고 젖은 종이를 눌러 폅니다.")
                },
                new[]
                {
                    A("soften-crease", "water-brush", "stiff-crease", "The crease relaxes. The fin stops tugging against the rest of the page.",
                        "접힌 선이 부드러워집니다. 팽팽하던 지느러미가 느슨해졌습니다.",
                        "Inspect the dry crease and dampen only the fold, away from the writing.", "굳은 접힌 자국을 살펴보고 글씨를 피해 선만 적시세요.", "repair", new[] { "stiff-crease" }),
                    A("unfold-fish", "paper-folder", "folded-fin", "The fin lifts, then the fish opens into a single page. No corners tear.",
                        "지느러미가 들리며 물고기가 편지 한 장으로 펼쳐집니다. 찢어진 곳은 없습니다.",
                        "Soften the crease first, then slide the folder beneath the fin.", "접힌 선을 부드럽게 만든 뒤 지느러미 밑에 주걱을 넣으세요.", "reveal", new[] { "folded-fin" }, new[] { "soften-crease" }),
                    A("dry-letter", "blotting-paper", "letter-lines", "The damp letter lies flat. Soyeon's message is clear, including where to leave a reply.",
                        "젖은 편지가 반듯하게 펴집니다. 소연 선배의 글이 선명해지고, 답장을 남길 곳도 읽을 수 있습니다.",
                        "Unfold the fish, inspect its writing, and press the damp page with blotting paper.", "물고기를 펼쳐 글씨를 살펴보고 젖은 종이를 흡수지로 누르세요.", "repair", new[] { "letter-lines" }, new[] { "unfold-fish" })
                });

        private static WorkbenchSceneDefinition KeptPromise()
            => new WorkbenchSceneDefinition(
                "rain-04-dry-order", "murmur-box",
                C("A reply at last", "이제 보내는 답장"),
                C("Open the letter compartment and prepare a reply.", "상자의 편지 칸을 열고 답장을 준비하세요."),
                C("I meant to answer her. Then one busy night became years. Could you open the letter compartment? I know what to write now.",
                    "답장을 쓰려고 했는데, 하루 이틀 미루다 몇 년이 됐네요. 편지 칸 좀 열어 주실래요? 이제 무슨 말을 할지 알겠어요."),
                C("'The umbrella is safe. We'll keep a place by the window.' Let's leave the reply where the rain can find it.",
                    "'우산은 잘 있어요. 창가의 자리도 남겨 둘게요.' 비가 찾을 수 있게 우산에 답장을 넣어 둬요."),
                C("The finished reply needs a dry pocket. The umbrella has one, but its stitching has come loose.",
                    "완성한 답장을 넣으려면 마른 주머니가 필요합니다. 우산 안쪽 주머니의 실밥이 풀려 있습니다."),
                new[]
                {
                    T("letter-lock", .57f, .35f, "Inner lock", "편지 칸 자물쇠",
                        "The lid is ajar, but the letter compartment is locked. Another crescent marks the inner lock.",
                        "뚜껑은 벌어졌지만 편지 칸은 잠겨 있습니다. 안쪽 자물쇠에도 초승달이 새겨져 있습니다."),
                    T("reply-sheet", .43f, .54f, "Reply card", "답장 카드",
                        "A blank reply waits beside the old note. The senior clerk takes a breath and begins to speak.",
                        "옛 편지 옆에 빈 답장 카드가 있습니다. 선임이 숨을 고르고 천천히 말을 시작합니다.", "open-compartment"),
                    T("envelope", .63f, .64f, "Open envelope", "열린 봉투",
                        "The envelope is dry inside, just large enough for the card. It would keep the writing away from the rain.",
                        "봉투 안은 말라 있고 카드가 꼭 맞는 크기입니다. 빗물이 글씨에 닿지 않게 해 주겠습니다.", "open-compartment")
                },
                new[]
                {
                    U("crescent-key", "Crescent key", "초승달 열쇠", "Also fits the old letter compartment.", "오래된 편지 칸에도 맞는 열쇠입니다."),
                    U("soft-pencil", "Soft pencil", "부드러운 연필", "Writes a reply on the waiting card.", "기다리던 카드에 답장을 씁니다."),
                    U("reply-card", "Reply card", "답장 카드", "Slides into its envelope once it is written.", "글을 쓴 뒤 봉투 안에 넣습니다.")
                },
                new[]
                {
                    A("open-compartment", "crescent-key", "letter-lock", "The inner lock opens. A voice says, 'A place out of the rain. That's all I ask.'",
                        "안쪽 자물쇠가 열립니다. '비를 피할 자리 하나. 그거면 돼.' 상자에서 목소리가 흘러나옵니다.",
                        "Inspect the inner lock and use the crescent key.", "편지 칸 자물쇠를 살펴보고 초승달 열쇠를 쓰세요.", "reveal", new[] { "letter-lock" }),
                    A("write-reply", "soft-pencil", "reply-sheet", "The pencil writes the senior clerk's reply: 'The umbrella is safe. We'll keep a place by the window.'",
                        "선임이 불러 주는 답장을 적습니다. '우산은 잘 있어요. 창가의 자리도 남겨 둘게요.'",
                        "Open the letter compartment and inspect the blank card before writing.", "편지 칸을 열고 빈 카드를 살펴본 다음 답장을 쓰세요.", "repair", new[] { "reply-sheet" }, new[] { "open-compartment" }),
                    A("pack-reply", "reply-card", "envelope", "The written card slips into the dry envelope. The voice box falls quiet.",
                        "글을 쓴 카드가 마른 봉투 안으로 들어갑니다. 상자의 목소리가 조용해집니다.",
                        "Write the reply first, then put the card into the inspected envelope.", "답장을 먼저 쓰고 봉투를 살펴본 뒤 카드를 넣으세요.", "warmth", new[] { "envelope" }, new[] { "write-reply" })
                });

        private static WorkbenchSceneDefinition APlaceOutOfTheRain()
            => new WorkbenchSceneDefinition(
                "rain-05-testimony", "moon-umbrella",
                C("A place out of the rain", "비를 피할 자리"),
                C("Make the pocket watertight and tuck the reply inside.", "주머니에 물이 새지 않게 고쳐 답장을 넣으세요."),
                C("One last repair. Keep the reply dry, then we can hang this umbrella by the door for whoever needs it next.",
                    "마지막으로 한 군데만 고쳐요. 답장이 젖지 않게 넣고 나면, 다음에 필요한 사람이 쓰도록 문가에 걸어 둡시다."),
                C("The rain has stopped. Thank you for staying with this. Tomorrow, when someone knocks, we'll have an umbrella ready.",
                    "비가 멎었네요. 끝까지 같이 봐줘서 고마워요. 내일 누가 문을 두드리면, 빌려줄 우산이 있겠어요."),
                C("The repaired umbrella waits by the door. A note on its handle reads: 'Borrow it. There's always someone here at night.'",
                    "고친 우산이 문가에서 기다립니다. 손잡이에 쪽지를 달았습니다. '빌려 쓰세요. 밤에도 사람이 있습니다.'"),
                new[]
                {
                    T("pocket-seam", .54f, .63f, "Loose pocket seam", "풀린 주머니 솔기",
                        "The pocket has opened along one edge. Drops slip between the loose stitches and wet the lining.",
                        "주머니 한쪽이 벌어졌습니다. 풀린 바늘땀 사이로 빗방울이 들어가 안감을 적십니다."),
                    T("dry-pocket", .39f, .31f, "Repaired pocket", "고친 주머니",
                        "Water runs past the new seam. Inside, the lining is dry and large enough for the envelope.",
                        "물이 새 솔기를 타고 흘러내립니다. 안감은 말라 있고 봉투가 들어갈 만큼 넉넉합니다.", "mend-pocket"),
                    T("carry-strap", .49f, .47f, "Carrying strap", "우산 고정 끈",
                        "The carrying loop is empty. The umbrella keeps slipping open, exposing the pocket to the rain.",
                        "고정 고리가 비었습니다. 우산이 자꾸 벌어져 주머니 쪽으로 비가 들어오려 합니다.")
                },
                new[]
                {
                    U("sewing-kit", "Needle and thread", "바늘과 실", "Repairs a pocket seam to keep paper dry.", "종이가 젖지 않도록 주머니 솔기를 고칩니다."),
                    U("sealed-reply", "Prepared reply", "봉투에 넣은 답장", "The senior clerk's reply, ready to put away.", "선임의 말을 적어 봉투에 넣은 답장입니다."),
                    U("soft-ribbon", "Soft ribbon", "부드러운 리본", "Fastens the umbrella without squeezing it.", "우산을 짓누르지 않고 고정합니다.")
                },
                new[]
                {
                    A("mend-pocket", "sewing-kit", "pocket-seam", "The new stitches close the seam. A drop runs over the pocket instead of into it.",
                        "새 바늘땀이 솔기를 막습니다. 빗방울이 주머니 안으로 스며들지 않고 흘러내립니다.",
                        "Inspect the loose pocket seam and stitch its open edge.", "풀린 주머니 솔기를 살펴보고 벌어진 가장자리를 꿰매세요.", "repair", new[] { "pocket-seam" }),
                    A("return-reply", "sealed-reply", "dry-pocket", "The reply settles into the dry pocket. 'It arrived,' says the rain, and the drops grow quiet.",
                        "답장이 마른 주머니에 들어갑니다. '잘 도착했네.' 비가 말하더니 빗방울 소리가 잦아듭니다.",
                        "Mend the pocket first, then inspect its dry lining before putting the reply inside.", "주머니를 고친 뒤 마른 안감을 확인하고 답장을 넣으세요.", "rain", new[] { "dry-pocket" }, new[] { "mend-pocket" }),
                    A("hang-umbrella", "soft-ribbon", "carry-strap", "You tie the ribbon and hang the umbrella by the door. The final drop stops. Warm lamplight fills the desk.",
                        "리본을 묶어 우산을 문가에 겁니다. 마지막 빗방울이 멎고 책상에 따뜻한 불빛이 퍼집니다.",
                        "Tuck the reply into its repaired pocket, then tie the ribbon through the carrying strap.", "고친 주머니에 답장을 넣은 다음 고정 끈에 리본을 묶으세요.", "warmth", new[] { "carry-strap" }, new[] { "return-reply" })
                });

        private static LocalizedCopy C(string english, string korean) => new LocalizedCopy(english, korean);

        private static WorkbenchTarget T(string id, float x, float y, string labelEnglish,
            string labelKorean, string observationEnglish, string observationKorean, string revealAfterStep = null)
            => new WorkbenchTarget(id, C(labelEnglish, labelKorean), C(observationEnglish, observationKorean),
                x, y, revealAfterStep);

        private static WorkbenchTool U(string id, string labelEnglish, string labelKorean,
            string descriptionEnglish, string descriptionKorean)
            => new WorkbenchTool(id, C(labelEnglish, labelKorean), C(descriptionEnglish, descriptionKorean));

        private static WorkbenchAction A(string id, string toolId, string targetId,
            string resultEnglish, string resultKorean, string hintEnglish, string hintKorean, string effect,
            string[] requiredObservations, string[] requiredSteps = null, string revealedArtifactId = null)
            => new WorkbenchAction(new WorkbenchStep(id, toolId, targetId, requiredObservations, requiredSteps),
                C(resultEnglish, resultKorean), C(hintEnglish, hintKorean), effect, revealedArtifactId);
    }
}
