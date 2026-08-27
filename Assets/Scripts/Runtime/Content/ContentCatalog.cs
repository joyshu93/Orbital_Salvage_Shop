using System;
using System.Collections.Generic;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;

namespace CurioClerk.Content
{
    public static class ContentCatalog
    {
        public const int ContentVersion = 2;

        public static IReadOnlyList<ArtifactContent> CreateArtifacts()
        {
            return new[]
            {
                A("clockwork-moth", "M", ArtifactTraits.Alive | ArtifactTraits.Metallic, "Clockwork Moth", "태엽 나방", "It remembers every lamp it has loved.", "사랑했던 모든 전등을 기억한다."),
                A("rain-jar", "J", ArtifactTraits.Wet | ArtifactTraits.Temporal, "Jar of Tuesday Rain", "화요일 빗물병", "The rain inside insists it is still Tuesday.", "병 속의 비는 아직 화요일이라고 우긴다."),
                A("whispering-key", "K", ArtifactTraits.Cursed | ArtifactTraits.Metallic, "Whispering Key", "속삭이는 열쇠", "It knows a door that no longer exists.", "이제는 없는 문 하나를 알고 있다."),
                A("sleeping-teacup", "T", ArtifactTraits.Alive | ArtifactTraits.Fragile, "Sleeping Teacup", "잠든 찻잔", "Do not wake it before the kettle sings.", "주전자가 노래하기 전에는 깨우지 말 것."),
                A("borrowed-shadow", "S", ArtifactTraits.Temporal | ArtifactTraits.Cursed, "Borrowed Shadow", "빌린 그림자", "Return before sunrise. It misses its owner.", "해 뜨기 전에 돌려줄 것. 주인을 그리워한다."),
                A("moon-umbrella", "U", ArtifactTraits.Wet | ArtifactTraits.Fragile, "Moon-Mended Umbrella", "달빛으로 기운 우산", "The patches glow whenever rain feels lonely.", "비가 외로울 때마다 기운 자리가 빛난다."),
                A("silent-bell", "B", ArtifactTraits.Cursed | ArtifactTraits.Metallic, "Bell Without a Tongue", "혀 없는 종", "It rings only in rooms that forgot you.", "당신을 잊은 방에서만 울린다."),
                A("mossy-watch", "W", ArtifactTraits.Alive | ArtifactTraits.Temporal | ArtifactTraits.Metallic, "Mossy Pocket Watch", "이끼 낀 회중시계", "The moss grows one minute ahead.", "이끼가 언제나 1분 먼저 자란다."),
                A("paper-fish", "F", ArtifactTraits.Alive | ArtifactTraits.Wet | ArtifactTraits.Fragile, "Paper Fish", "종이 물고기", "Folded from a letter nobody sent.", "아무도 보내지 않은 편지로 접혔다."),
                A("backward-candle", "C", ArtifactTraits.Temporal | ArtifactTraits.Fragile, "Backward Candle", "거꾸로 타는 양초", "Its wax climbs toward an earlier evening.", "촛농이 더 이른 저녁을 향해 올라간다."),
                A("porcelain-tooth", "P", ArtifactTraits.Fragile | ArtifactTraits.Cursed, "Porcelain Tooth", "도자기 이빨", "Too polite to bite, too rude to stop smiling.", "물기엔 공손하고 웃음을 멈추기엔 무례하다."),
                A("thimble-storm", "R", ArtifactTraits.Wet | ArtifactTraits.Metallic, "Thimble Storm", "골무 속 폭풍", "A tiny forecast with very loud opinions.", "목소리 큰 아주 작은 일기예보다."),
                A("humming-scarf", "H", ArtifactTraits.Alive | ArtifactTraits.Cursed, "Humming Scarf", "콧노래 목도리", "It only knows the chorus of a forbidden song.", "금지된 노래의 후렴만 알고 있다."),
                A("sundial-egg", "E", ArtifactTraits.Alive | ArtifactTraits.Temporal | ArtifactTraits.Fragile, "Sundial Egg", "해시계 알", "Something inside is waiting for noon.", "안의 무언가가 정오를 기다린다."),
                A("mirror-seed", "D", ArtifactTraits.Cursed | ArtifactTraits.Fragile, "Mirror Seed", "거울 씨앗", "Plant reflection-side down.", "반사되는 쪽을 아래로 심을 것."),
                A("rusty-comet", "O", ArtifactTraits.Metallic | ArtifactTraits.Temporal, "Rusty Comet", "녹슨 혜성", "Late by three centuries and terribly embarrassed.", "세 세기 늦어서 몹시 난처해한다."),
                A("ink-snowglobe", "G", ArtifactTraits.Wet | ArtifactTraits.Cursed | ArtifactTraits.Fragile, "Ink Snow Globe", "잉크 스노글로브", "Shake gently. The forecast is confidential.", "살살 흔들 것. 예보는 기밀이다."),
                A("patient-compass", "Q", ArtifactTraits.Metallic | ArtifactTraits.Alive, "Patient Compass", "참을성 많은 나침반", "It points home when you are ready.", "당신이 준비되면 집을 가리킨다."),
                A("yesterday-ticket", "Y", ArtifactTraits.Temporal | ArtifactTraits.Fragile, "Yesterday Ticket", "어제행 승차권", "One way. No refunds for paradoxes.", "편도. 역설로 인한 환불은 불가하다."),
                A("tea-crown", "N", ArtifactTraits.Cursed | ArtifactTraits.Metallic | ArtifactTraits.Wet, "Tea-Stained Crown", "찻물 얼룩 왕관", "Abdicated after a difficult breakfast.", "힘든 아침 식사 후 퇴위했다."),
                A("lantern-snail", "L", ArtifactTraits.Alive | ArtifactTraits.Fragile, "Lantern Snail", "등불 달팽이", "Carries a warm porch wherever it goes.", "어디를 가든 따뜻한 현관을 등에 진다."),
                A("tide-locket", "I", ArtifactTraits.Wet | ArtifactTraits.Metallic, "Tide-Locked Locket", "밀물에 잠긴 로켓", "Opens only when the moon approves.", "달이 허락할 때만 열린다."),
                A("murmur-box", "X", ArtifactTraits.Cursed | ArtifactTraits.Alive | ArtifactTraits.Metallic, "Murmur Box", "웅얼거림 상자", "Compliments are accepted through the keyhole.", "열쇠구멍으로 칭찬을 받는다."),
                A("unmelting-ice", "V", ArtifactTraits.Wet | ArtifactTraits.Temporal | ArtifactTraits.Fragile, "Unmelting Ice", "녹지 않는 얼음", "Frozen during a winter that has not happened yet.", "아직 오지 않은 겨울에 얼었다.")
            };
        }

        public static IReadOnlyList<SortingRule> CreateRuleTemplates()
        {
            return new[]
            {
                R("cursed-vault", ArtifactTraits.Cursed, ArtifactTraits.None, Destination.Vault),
                R("alive-vault", ArtifactTraits.Alive, ArtifactTraits.None, Destination.Vault),
                R("temporal-vault", ArtifactTraits.Temporal, ArtifactTraits.None, Destination.Vault),
                R("fragile-repair", ArtifactTraits.Fragile, ArtifactTraits.None, Destination.Repair),
                R("wet-repair", ArtifactTraits.Wet, ArtifactTraits.None, Destination.Repair),
                R("metallic-repair", ArtifactTraits.Metallic, ArtifactTraits.None, Destination.Repair),
                R("cursed-metal-vault", ArtifactTraits.Cursed | ArtifactTraits.Metallic, ArtifactTraits.None, Destination.Vault),
                R("living-wet-vault", ArtifactTraits.Alive | ArtifactTraits.Wet, ArtifactTraits.None, Destination.Vault),
                R("fragile-time-repair", ArtifactTraits.Fragile | ArtifactTraits.Temporal, ArtifactTraits.None, Destination.Repair),
                R("wet-or-metal-storage", ArtifactTraits.None, ArtifactTraits.Wet | ArtifactTraits.Metallic, Destination.Storage)
            };
        }

        public static IReadOnlyList<SortingRule> CreateRulesForBand(int band, int seed)
        {
            if (band < 1 || band > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(band));
            }

            var templates = new List<SortingRule>(CreateRuleTemplates());
            var random = new Random(seed ^ (band * 397));
            for (var index = templates.Count - 1; index > 0; index--)
            {
                var swap = random.Next(index + 1);
                var item = templates[index];
                templates[index] = templates[swap];
                templates[swap] = item;
            }

            var activeCount = band <= 2 ? 2 : band <= 4 ? 3 : 4;
            var active = templates.GetRange(0, activeCount);
            active.Add(new SortingRule("fallback-storage", ArtifactTraits.None, ArtifactTraits.None, Destination.Storage, true));
            return active;
        }

        public static IReadOnlyList<ShiftRulePack> CreateRulePacks()
        {
            return new[]
            {
                new ShiftRulePack("pack-cursed-fragile", 1, 3, new[]
                {
                    R("cursed-vault", ArtifactTraits.Cursed, ArtifactTraits.None, Destination.Vault),
                    R("fragile-repair", ArtifactTraits.Fragile, ArtifactTraits.None, Destination.Repair),
                    new SortingRule("fallback-storage", ArtifactTraits.None, ArtifactTraits.None,
                        Destination.Storage, true)
                }),
                new ShiftRulePack("pack-temporal-wet", 2, 3, new[]
                {
                    R("temporal-vault", ArtifactTraits.Temporal, ArtifactTraits.None, Destination.Vault),
                    R("wet-repair", ArtifactTraits.Wet, ArtifactTraits.None, Destination.Repair),
                    new SortingRule("fallback-storage", ArtifactTraits.None, ArtifactTraits.None,
                        Destination.Storage, true)
                })
            };
        }

        public static IReadOnlyList<ShiftSequenceTemplate> CreateShiftTemplates()
        {
            return new[]
            {
                T("docket-band-1", 1, 1, 1,
                    Destination.Repair, Destination.Repair, Destination.Storage, Destination.Vault,
                    Destination.Repair, Destination.Storage, Destination.Vault, Destination.Repair,
                    Destination.Storage, Destination.Vault, Destination.Storage, Destination.Vault),
                T("docket-band-2", 2, 2, 2,
                    Destination.Vault, Destination.Vault, Destination.Repair, Destination.Storage,
                    Destination.Repair, Destination.Storage, Destination.Vault, Destination.Storage,
                    Destination.Storage, Destination.Repair, Destination.Vault, Destination.Repair),
                T("docket-band-3", 3, 3, 3,
                    Destination.Repair, Destination.Repair, Destination.Storage, Destination.Vault,
                    Destination.Storage, Destination.Storage, Destination.Vault, Destination.Repair,
                    Destination.Repair, Destination.Vault, Destination.Storage, Destination.Vault)
            };
        }

        public static IReadOnlyList<CosmeticContent> CreateCosmetics()
        {
            return new[]
            {
                new CosmeticContent("brass-lamp", 80, "Brass Lamp", "황동 램프", "D6A85F"),
                new CosmeticContent("moth-mobile", 160, "Moth Mobile", "나방 모빌", "C88B8B"),
                new CosmeticContent("plum-runner", 260, "Plum Desk Runner", "자두색 책상보", "6E334F"),
                new CosmeticContent("moon-mug", 380, "Moon Mug", "달 머그잔", "8094B8"),
                new CosmeticContent("fern-familiar", 520, "Fern Familiar", "고사리 패밀리어", "718B63"),
                new CosmeticContent("amber-window", 700, "Amber Window", "호박빛 창문", "E0A24B")
            };
        }

        private static ArtifactContent A(string id, string symbol, ArtifactTraits traits, string en, string ko, string descEn, string descKo)
            => new ArtifactContent(id, symbol, traits, en, ko, descEn, descKo);

        private static SortingRule R(string id, ArtifactTraits all, ArtifactTraits any, Destination destination)
            => new SortingRule(id, all, any, destination, false);

        private static ShiftSequenceTemplate T(
            string id,
            int minimumBand,
            int maximumBand,
            int minimumHolds,
            params Destination[] destinations)
            => new ShiftSequenceTemplate(
                id,
                minimumBand,
                maximumBand,
                minimumHolds,
                destinations);
    }
}
