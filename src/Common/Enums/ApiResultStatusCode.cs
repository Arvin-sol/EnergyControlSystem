

using System.ComponentModel.DataAnnotations;

namespace Common.Enums;

public enum ApiResultStatusCode:byte
{
    [Display(Name = "عملیات با موفقیت انجام شد")] Success = 0,


    [Display(Name = "کد خطا:1 _ خطایی در سرور رخ داده است")] ServerError = 1,
    [Display(Name = "کد خطا:2 _ پارامتر های ارسالی معتبر نیستند")] BadRequest = 2,

    [Display(Name = "کد خطا:3 _ یافت نشد")] NotFound = 3,
    [Display(Name = "کد خطا:6 _ خطای احراز هویت")] UnAuthorized = 6,
    [Display(Name = "اطلاعات قبلا ثبت شده است")] InformationExists = 8,
}
