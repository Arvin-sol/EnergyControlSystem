using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.EquipmentAggregate.Enums;



public static class EquipmentEnums
{
    public enum EquipmentType : byte
    {
        [Display(Name = "تخلیه")] HVAC = 1,
        [Display(Name = "تخلیه")] Elevator = 2,
        [Display(Name = "تخلیه")] Lighting = 3,
        [Display(Name = "تخلیه")] Other = 4,
    }

    public enum Status : byte
    {
        [Display(Name = "تخلیه")] Online = 1,
        [Display(Name = "تخلیه")] Offline = 2,
        [Display(Name = "تخلیه")] Maintenance = 3,
        [Display(Name = "تخلیه")] Faulty = 4,
    }
}

