export interface InverterInfo {
    backToBattery: number;
    batteryCutOffVoltage: number;
    batteryKwhDischargedLast24Hr: number;
    batteryLoadInWatts: number;
    batteryLowBackToGrid: number;
    batteryVoltage: number;
    bulkVoltage: number;
    chargingCurrentInAmps: number;
    chargingSource: number;
    floatVoltage: number;
    gridChargeInKwhToBatteryLast24Hr: number;
    gridKwhDischargedLast24Hr: number;
    gridLoadInWatts: number;
    inverterName: string;
    loadInWatts: number;
    loadPercentage: number;
    outputSource: number;
    solarGenerationLast24hrInkWh: number;
    status: number;
    temperature: number;
    batteryACChargeInWatts: number;
    pvPowerInWatts: number;
    pvVoltage: number;
}