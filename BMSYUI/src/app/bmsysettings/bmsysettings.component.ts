import { Component, OnInit } from '@angular/core';
import { InverterInfo } from '../inverterinfo';
import { BmsyapiService } from '../bmsyapi.service';
import { interval } from 'rxjs';
import { BMSInfo } from '../bmsinfo';

@Component({
  selector: 'app-bmsysettings',
  templateUrl: './bmsysettings.component.html',
  styleUrls: ['./bmsysettings.component.css']
})

export class BMSYSettingsComponent implements OnInit {

  constructor(private bmsyapiService: BmsyapiService) { }

  ngOnInit(): void {
    this.getData();
    interval(5000).subscribe(() => { this.getData(); });

  }

  growatts: InverterInfo[] = [];
  bulkvoltage: string = "";
  floatvoltage: string = "";
  battcutoff: string = "";
  battlowbacktogrid: string = "";
  backtobatt: string ="";
  selectedGrowatt?: InverterInfo;
  selectedBMS?: BMSInfo;
  outputSource: string = "";
  chargingSource: string = "";
  chargingCurrent: string = "";
  bmses: BMSInfo[] = [];

  reloadVoltages: boolean = true;



  getGrowatts(): void {
    this.bmsyapiService.getInverterInfo().subscribe(resultaat => {

      if (resultaat.length > 0) {
        this.growatts = resultaat;
        this.battlowbacktogrid = resultaat[0].batteryLowBackToGrid.toString();

        this.chargingCurrent = resultaat[0].chargingCurrentInAmps.toString();
        this.chargingSource = resultaat[0].chargingSource.toString();

        if (this.reloadVoltages) {
          this.reloadVoltages = false;
          this.floatvoltage = resultaat[0].floatVoltage.toString();
          this.bulkvoltage = resultaat[0].bulkVoltage.toString();
          this.outputSource = resultaat[0].outputSource.toString();
          this.backtobatt = resultaat[0].backToBattery.toString();
          this.battlowbacktogrid = resultaat[0].batteryLowBackToGrid.toString();
          this.battcutoff = resultaat[0].batteryCutOffVoltage.toString();
        }
      }
    });
  }


  getBMSES(): void {
    this.bmsyapiService.getBMSInfo().subscribe(resultaat => {

      if (resultaat.length > 0) {
        this.bmses = resultaat;
      }
    });
  }


  onFloatSet(): void {
    this.bmsyapiService.setFloatVoltage(this.floatvoltage);
    this.reloadVoltages = true;
    this.getData();
  }

  onBulkSet(): void {
    console.log(this.bulkvoltage);
    this.bmsyapiService.setBulkVoltage(this.bulkvoltage);
    this.reloadVoltages = true;
    this.getData();

  }
  onbackToGridSet(): void {
    console.log(this.battlowbacktogrid);
    this.bmsyapiService.setBatteryLowBackToGrid(this.battlowbacktogrid);
    this.reloadVoltages = true;
    this.getData();
  }
  onbackToBattSet(): void {
    console.log(this.backtobatt);
    this.bmsyapiService.setBackToBatt(this.backtobatt);
    this.reloadVoltages = true;
    this.getData();
  }
  onbattCutOffSet(): void {
    console.log(this.battcutoff);
    this.bmsyapiService.setBattCuttOff(this.battcutoff);
    this.reloadVoltages = true;
    this.getData();
  }

  getData():void{
    this.getGrowatts();
    this.getBMSES();
  }

  onSelectInverter(hero: InverterInfo): void {
    this.selectedGrowatt = hero;
  }
  
  onSelectBMS(hero: BMSInfo): void {
    this.selectedBMS = hero;
  }
  
 

  getNumbers(start: number, stop: number) {
    const a: any[] = [];
    for (let i = start; i < stop; i++) {
      a.push(`${i}`);
      for (let y = 1; y < 10; y++) {
        a.push(`${i}.${y}`);
      }
    }
    return a;
  }

  getInt(num: string) : number{
    return parseInt(num);
  }

  trackByIdentity(index: number, item: InverterInfo) {
    return item.inverterName;
  }

  tracBMSkByIdentity(index: number, item: BMSInfo) {
    return item.bmsName;
  }

  setChargingCurrent(value: number) {
    this.bmsyapiService.setChargingCurrent(value);
    this.getData();
  }

  setChargingSource(value: number) {
    this.bmsyapiService.setChargingSource(value);
    this.getData();
  }

  setOutputSource(value: number) {
    this.bmsyapiService.setOutputSource(value);
    this.getData();
  }

  fuckUnderscores(thisthing: string): string {
    return thisthing.replace(/_/g, ' ');
  }

  getOutputSource(id: number): string {
    if (id == 0)
      return "Sol/Batt/Utility";
    if (id == 1)
      return "Solar First";
      if (id == 2)
      return "Utility First";      
    if (id == 3)
      return "Sol/Utility/Batt";
    return "DUNNO!";
  }

  getChargingSource(id: number): string {
    if (id == 0)
      return "Solar First Charging";
    if (id == 1)
      return "Solar and Utility";
    if (id == 2)
      return "Solar Only";
    return "DUNNO!";
  }

  getStatus(inverter: InverterInfo): string {
    if (inverter.status == 2)
      return "Discharge";
    if (inverter.status == 3)
      return "Fault";
    if (inverter.status == 5)
      return "PV Charging";
    if (inverter.status == 6)
      return "AC Charging";
    if (inverter.status == 7)
      return "Combine_Charge";
    if (inverter.status == 8)
      return "Combine Charge and_Bypass";
    if (inverter.status == 9)
      return "PV Charge and Bypass";
    if (inverter.status == 10)
      return "AC Charge and Bypass";
    if (inverter.status == 11)
      return "Bypass";
    if (inverter.status == 12)
      return "PV Charge and Discharge";

    return "Fucked if I know: " + inverter.status;

  }
}
