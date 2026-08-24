using System.ComponentModel.DataAnnotations;

namespace BodyFat.Models
{
    public class BodyFatInputModel
    {
        [Required(ErrorMessage = "Вкажіть вік")]
        [Display(Name = "Вік (років)")]
        public int Age { get; set; } = 30;

        [Required(ErrorMessage = "Вкажіть вагу")]
        [Display(Name = "Вага (кг)")]
        public string WeightKg { get; set; } = "70";

        [Required(ErrorMessage = "Вкажіть зріст")]
        [Display(Name = "Зріст (см)")]
        public string HeightCm { get; set; } = "175";

        [Display(Name = "Шия (см)")]
        public string NeckCm { get; set; } = "37";

        [Display(Name = "Груди (см)")]
        public string ChestCm { get; set; } = "95";

        [Display(Name = "Живіт (см)")]
        public string AbdomenCm { get; set; } = "82";

        [Display(Name = "Стегна/Таз (см)")]
        public string HipCm { get; set; } = "95";

        [Display(Name = "Стегно (см)")]
        public string ThighCm { get; set; } = "55";

        [Display(Name = "Коліно (см)")]
        public string KneeCm { get; set; } = "37";

        [Display(Name = "Кісточка (см)")]
        public string AnkleCm { get; set; } = "22";

        [Display(Name = "Біцепс (см)")]
        public string BicepsCm { get; set; } = "32";

        [Display(Name = "Передпліччя (см)")]
        public string ForearmCm { get; set; } = "28";

        [Display(Name = "Зап'ястя (см)")]
        public string WristCm { get; set; } = "18";
    }
}