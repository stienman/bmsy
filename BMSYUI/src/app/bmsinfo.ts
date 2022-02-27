export interface BMSInfo {
    cellAverage: number;
    cellDifference: number;
    cellHigh: number;
    cellLow: number;
    cellVoltages: number[];
    current: number;
    cycles: number;
    protectionState: number;
    soc: number;
    temperatures: number[];
    voltage: number;
    bmsName: string;
}