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
            ["daily_available"] = "Available",
            ["daily_completed"] = "Completed · Best {0}",
            ["daily_badge"] = "DAILY FILE · {0}",
            ["daily_result_best"] = "Today's best: {0}",
            ["collection"] = "Casebook",
            ["settings"] = "Settings",
            ["tutorial_title"] = "Three desks. One rulebook.",
            ["tutorial_body"] = "Read rules from top to bottom. The first matching rule wins. Training mistakes do not cost hearts, so take your time and try Hold.",
            ["tutorial_controls"] = "REPAIR · STORAGE · VAULT\nUse HOLD when you need a second look.",
            ["tutorial_step_one"] = "1 / 4 · FRAGILE goes to REPAIR. File the teacup.",
            ["tutorial_step_two"] = "2 / 4 · Two rules match. The first rule wins: REPAIR.",
            ["tutorial_step_hold"] = "3 / 4 · Put this item on HOLD before sorting.",
            ["tutorial_step_after_hold"] = "3 / 4 · Now use CURSED to file the revealed key.",
            ["tutorial_step_final"] = "4 / 4 · Final check. Sort the held item without a hint.",
            ["tutorial_hold_first"] = "HOLD FIRST · Use the Hold button before sorting.",
            ["tutorial_follow_step"] = "FOLLOW THE NOTE · Complete the current lesson first.",
            ["tutorial_complete_title"] = "Training Complete",
            ["tutorial_complete_body"] = "The rulebook is yours. Your first full night shift is ready.",
            ["tutorial_start_shift"] = "Start First Shift",
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
            ["feedback_settings"] = "Feedback",
            ["sound"] = "Sound",
            ["haptics"] = "Haptics",
            ["on"] = "On",
            ["off"] = "Off",
            ["privacy"] = "Privacy",
            ["casebook_empty"] = "Correctly sort curios to reveal their case files.",
            ["casebook_tab"] = "CASEBOOK",
            ["casebook_discovered"] = "{0} / {1} DISCOVERED",
            ["casebook_locked"] = "LOCKED CASE FILE",
            ["cosmetics"] = "DESK CHARMS",
            ["cosmetics_tab"] = "DESK CHARMS",
            ["collection_coins"] = "COINS {0}",
            ["cosmetic_unlock_status"] = "UNLOCK · {0} COINS",
            ["cosmetic_equip_status"] = "EQUIP",
            ["cosmetic_equipped_status"] = "EQUIPPED",
            ["cosmetic_equipped_feedback"] = "{0} equipped",
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
            ["daily_available"] = "도전 가능",
            ["daily_completed"] = "완료 · 최고 {0}",
            ["daily_badge"] = "오늘의 서류 · {0}",
            ["daily_result_best"] = "오늘의 최고 점수: {0}",
            ["collection"] = "수집 도감",
            ["settings"] = "설정",
            ["tutorial_title"] = "세 곳의 보관처, 한 권의 규칙표",
            ["tutorial_body"] = "규칙을 위에서 아래로 읽으세요. 먼저 일치하는 규칙이 정답입니다. 교육 중에는 실수해도 하트가 줄지 않으니 천천히 보류도 사용해 보세요.",
            ["tutorial_controls"] = "수리실 · 보관실 · 봉인고\n헷갈리는 물건은 보류할 수 있습니다.",
            ["tutorial_step_one"] = "1 / 4 · 깨지기 쉬움은 수리실입니다. 찻잔을 분류하세요.",
            ["tutorial_step_two"] = "2 / 4 · 두 규칙이 맞으면 위쪽 규칙이 우선입니다: 수리실.",
            ["tutorial_step_hold"] = "3 / 4 · 분류하기 전에 이 물건을 보류하세요.",
            ["tutorial_step_after_hold"] = "3 / 4 · 이제 저주받음 규칙으로 드러난 열쇠를 분류하세요.",
            ["tutorial_step_final"] = "4 / 4 · 마지막 확인입니다. 도움 없이 보류한 물건을 분류하세요.",
            ["tutorial_hold_first"] = "먼저 보류 · 분류하기 전에 보류 버튼을 누르세요.",
            ["tutorial_follow_step"] = "안내 확인 · 현재 단계를 먼저 완료하세요.",
            ["tutorial_complete_title"] = "교육 완료",
            ["tutorial_complete_body"] = "이제 규칙표를 맡을 준비가 됐습니다. 첫 정식 교대를 시작하세요.",
            ["tutorial_start_shift"] = "첫 교대 시작",
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
            ["feedback_settings"] = "게임 반응",
            ["sound"] = "소리",
            ["haptics"] = "진동",
            ["on"] = "켜짐",
            ["off"] = "꺼짐",
            ["privacy"] = "개인정보",
            ["casebook_empty"] = "기물을 올바르게 분류하면 기록이 공개됩니다.",
            ["casebook_tab"] = "수집 도감",
            ["casebook_discovered"] = "{0} / {1} 발견",
            ["casebook_locked"] = "잠긴 기록",
            ["cosmetics"] = "책상 장식",
            ["cosmetics_tab"] = "책상 장식",
            ["collection_coins"] = "코인 {0}",
            ["cosmetic_unlock_status"] = "해금 · {0} 코인",
            ["cosmetic_equip_status"] = "장착",
            ["cosmetic_equipped_status"] = "장착 중",
            ["cosmetic_equipped_feedback"] = "{0} 장착 완료",
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
