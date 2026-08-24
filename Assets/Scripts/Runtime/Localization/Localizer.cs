using System.Collections.Generic;

namespace CurioClerk.Localization
{
    public sealed class Localizer
    {
        private static readonly IReadOnlyDictionary<string, string> English = new Dictionary<string, string>
        {
            ["title"] = "CURIO CLERK",
            ["subtitle"] = "NIGHT SHIFT",
            ["start"] = "Start Shift",
            ["daily"] = "Daily File",
            ["collection"] = "Casebook",
            ["settings"] = "Settings",
            ["tutorial_title"] = "Three desks. One rulebook.",
            ["tutorial_body"] = "Read rules from top to bottom. The first matching rule wins. Hold one item when you need a second look. Three mistakes end the shift.",
            ["begin"] = "Clock In",
            ["repair"] = "REPAIR",
            ["storage"] = "STORAGE",
            ["vault"] = "VAULT",
            ["hold"] = "HOLD",
            ["next"] = "NEXT",
            ["rules"] = "TONIGHT'S RULES",
            ["fallback"] = "Otherwise → Storage",
            ["correct"] = "Filed neatly.",
            ["wrong"] = "Wrong desk. Correct: {0}",
            ["feedback_correct_label"] = "CORRECT",
            ["feedback_wrong_label"] = "WRONG",
            ["result_correct_label"] = "CORRECT",
            ["result_mistakes_label"] = "MISTAKES",
            ["complete"] = "Shift Complete",
            ["failed"] = "Office Closed Early",
            ["continue"] = "Return to Desk",
            ["revive"] = "Watch ad: restore 1 heart",
            ["double"] = "Watch ad: double base coins",
            ["ad_unavailable"] = "Rewarded ad unavailable",
            ["ad_dismissed"] = "Ad dismissed. No changes were made.",
            ["ad_failed"] = "Ad failed. No changes were made.",
            ["coins"] = "Coins",
            ["score"] = "Score",
            ["back"] = "Back",
            ["language"] = "Language",
            ["privacy"] = "Privacy",
            ["casebook_empty"] = "Correctly sort curios to reveal their case files.",
            ["cosmetics"] = "DESK CHARMS",
            ["owned"] = "Owned",
            ["equip"] = "Equip",
            ["equipped"] = "Equipped",
            ["insufficient"] = "Not enough coins",
            ["unlock"] = "Unlock {0} coins",
            ["privacy_options"] = "Ad privacy options",
            ["trait_cursed"] = "CURSED",
            ["trait_fragile"] = "FRAGILE",
            ["trait_alive"] = "ALIVE",
            ["trait_temporal"] = "TEMPORAL",
            ["trait_wet"] = "WET",
            ["trait_metallic"] = "METALLIC"
        };

        private static readonly IReadOnlyDictionary<string, string> Korean = new Dictionary<string, string>
        {
            ["title"] = "기묘한 분실물",
            ["subtitle"] = "야간반",
            ["start"] = "교대 시작",
            ["daily"] = "오늘의 서류",
            ["collection"] = "수집 도감",
            ["settings"] = "설정",
            ["tutorial_title"] = "세 곳의 보관처, 한 권의 규칙표",
            ["tutorial_body"] = "규칙을 위에서 아래로 읽으세요. 먼저 일치하는 규칙이 정답입니다. 헷갈리는 물건 하나는 보류할 수 있습니다. 세 번 실수하면 교대가 끝납니다.",
            ["begin"] = "출근하기",
            ["repair"] = "수리실",
            ["storage"] = "보관실",
            ["vault"] = "봉인고",
            ["hold"] = "보류",
            ["next"] = "다음",
            ["rules"] = "오늘 밤의 규칙",
            ["fallback"] = "그 외 → 보관실",
            ["correct"] = "깔끔하게 처리했습니다.",
            ["wrong"] = "잘못 분류했습니다. 정답: {0}",
            ["feedback_correct_label"] = "정답",
            ["feedback_wrong_label"] = "오답",
            ["result_correct_label"] = "정답",
            ["result_mistakes_label"] = "실수",
            ["complete"] = "교대 완료",
            ["failed"] = "조기 폐점",
            ["continue"] = "책상으로 돌아가기",
            ["revive"] = "광고 시청: 하트 1개 회복",
            ["double"] = "광고 시청: 기본 코인 2배",
            ["ad_unavailable"] = "현재 보상형 광고를 이용할 수 없습니다",
            ["ad_dismissed"] = "광고가 닫혔습니다. 변경된 내용이 없습니다.",
            ["ad_failed"] = "광고를 재생하지 못했습니다. 변경된 내용이 없습니다.",
            ["coins"] = "코인",
            ["score"] = "점수",
            ["back"] = "뒤로",
            ["language"] = "언어",
            ["privacy"] = "개인정보",
            ["casebook_empty"] = "기물을 올바르게 분류하면 기록이 공개됩니다.",
            ["cosmetics"] = "책상 장식",
            ["owned"] = "보유 중",
            ["equip"] = "장착",
            ["equipped"] = "장착 중",
            ["insufficient"] = "코인이 부족합니다",
            ["unlock"] = "{0} 코인으로 해금",
            ["privacy_options"] = "광고 개인정보 설정",
            ["trait_cursed"] = "저주받음",
            ["trait_fragile"] = "깨지기 쉬움",
            ["trait_alive"] = "살아 있음",
            ["trait_temporal"] = "시간성",
            ["trait_wet"] = "젖어 있음",
            ["trait_metallic"] = "금속성"
        };

        public Localizer(string locale)
        {
            Locale = locale == "ko" ? "ko" : "en";
        }

        public string Locale { get; private set; }

        public void SetLocale(string locale)
        {
            Locale = locale == "ko" ? "ko" : "en";
        }

        public string Get(string key, params object[] arguments)
        {
            var table = Locale == "ko" ? Korean : English;
            var value = table.TryGetValue(key, out var localized) ? localized : key;
            return arguments == null || arguments.Length == 0 ? value : string.Format(value, arguments);
        }

        public static IEnumerable<KeyValuePair<string, string>> Entries(string locale)
            => locale == "ko" ? Korean : English;
    }
}
