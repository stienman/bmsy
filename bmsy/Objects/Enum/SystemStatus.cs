#nullable enable
using System.Runtime.Serialization;

public enum SystemStatus
{
    [EnumMember(Value = "Standby")]
    Standby = 0,
    [EnumMember(Value = "NOP")]
    NOP = 1,
    [EnumMember(Value = "Discharge")]
    Discharge = 2,
    [EnumMember(Value = "Fault")]
    Fault = 3,
    [EnumMember(Value = "Flash")]
    Flash = 4,
    [EnumMember(Value = "PV Charge")]
    PV_Charge = 5,
    [EnumMember(Value = "AC Charge")]
    AC_Charge = 6,
    [EnumMember(Value = "Combine Charge")]
    Combine_Charge = 7,
    [EnumMember(Value = "Combine Charge and Bypass")]
    Combine_Charge_and_Bypass = 8,
    [EnumMember(Value = "PV_Charge and Bypass")]
    PV_Charge_and_Bypass = 9,
    [EnumMember(Value = "AC Charge and Bypass")]
    AC_Charge_and_Bypass = 10,
    [EnumMember(Value = "Bypass")]
    Bypass = 11,
    [EnumMember(Value = "PV Charge and Discharge")]
    PV_Charge_and_Discharge = 12
}
