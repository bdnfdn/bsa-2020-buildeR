import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '@core/services/authentication.service';
import { User } from '@shared/models/user/user';
import { Project } from '@shared/models/project/project';
import { BuildHistory } from '@shared/models/build-history';
import { BuildStatus } from '@shared/models/build-status';
import { BuildHistoryService } from '@core/services/build-history.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  templateUrl: './insights.component.html',
  styleUrls: ['./insights.component.sass']
})
export class InsightsComponent implements OnInit {
  user: User = this.authService.getCurrentUser();
  now: Date = new Date(Date.now());
  countedDate = new Date(this.user.createdAt);
  totalBuilds = 0;
  totalDuration = 0;
  buildSuccessRate = 0;
  activeProjects = 0;
  tab = 0;
  month = false;

  buildsData;
  durationData;
  successData;
  bigChartData;
  projectData;

  weekMonth = [
    { name: 'Week' },
    { name: 'Month' },
  ];

  constructor(private authService: AuthenticationService, private route: ActivatedRoute) {
    this.route.parent.data.subscribe(data => this.user = data.user);
   }

  ngOnInit(): void {
    this.totalBuilds = this.totalBuildsCount();
    this.totalDuration = this.user.buildHistories.length ?
    this.user.buildHistories.map(this.duration).reduce(this.sum) : 0;
    this.buildSuccessRate = this.buildSucceedCount();
    this.activeProjects = this.countActiveProjects();
    this.countActiveProjects();
    this.getData();
  }

  getData(isMonth = false) {
    const diff = this.diffDates(this.now, this.user.createdAt);
    if (diff <= 7) {
      this.countedDate = this.user.createdAt;
      this.fulfillCharts(this.user.createdAt, diff);
      return;
    }
    if (!(diff <= 7) && isMonth){
      // Show month
      const date = new Date(this.now);
      this.countedDate = new Date(this.now);
      this.countedDate.setDate(this.countedDate.getDate() - 30);
      date.setDate(date.getDate() - 30);
      this.fulfillCharts(date, 30);
      return;
    }
    // Show week
    const date = new Date(this.now);
    this.countedDate = new Date(this.now);
    this.countedDate.setDate(this.countedDate.getDate() - 6);
    date.setDate(date.getDate() - 6);
    this.fulfillCharts(date, 6);
    return;
  }

  fulfillCharts(startDate: Date, days: number){
    this.buildsData = this.formatBuildsData(startDate, days);
    this.durationData = this.formatDurationData(startDate, days);
    this.successData = this.formatSuccessData(startDate, days);
    this.projectData = this.formatProjectData(startDate, days);
    this.bigChartData = this.formatBigChartData(startDate, days);
    return;
  }

  formatBigChartData(startDate: Date, days: number) {
    const result = new Array();
    startDate = new Date(startDate);
    for (let index = 0; index <= days; index++) {
      const newDay = new Date(startDate);
      newDay.setDate(newDay.getDate() + index);
      // name for result
      const day = this.beautifyDate(newDay);
      // counting successfull builds
      const successCount = this.countBuildsByStatus(newDay, BuildStatus.Success);
      const failCount = this.countBuildsByStatus(newDay, BuildStatus.Failure);
      const errorCount = this.countBuildsByStatus(newDay, BuildStatus.Error);
      const canceledCount = this.countBuildsByStatus(newDay, BuildStatus.Canceled);
      const value = [
        { name: 'Succeed', value: successCount },
        { name: 'Failed', value: failCount },
        { name: 'Errored', value: errorCount },
        { name: 'Canceled', value: canceledCount }];

      result.push({ name: day, series: value });
    }
    return result;
  }

  formatProjectData(startDate: Date, days: number) {
    const result = new Array();
    startDate = new Date(startDate);
    for (let index = 0; index <= days; index++) {
      const newDay = new Date(startDate);
      newDay.setDate(newDay.getDate() + index);
      const value = this.countActiveProjectsInDay(newDay);
      const name = this.beautifyDate(newDay);
      result.push({ name, value });
    }
    return [{ name: 'projects', series: result }];
  }

  formatBuildsData(startDate: Date, days: number) {
    const result = new Array();
    startDate = new Date(startDate);
    for (let index = 0; index <= days; index++) {
      const newDay = new Date(startDate);
      newDay.setDate(newDay.getDate() + index);
      const value = this.countBuildsInDay(newDay);
      const name = this.beautifyDate(newDay);
      result.push({ name, value });
    }
    return [{ name: 'builds', series: result }];
  }

  formatDurationData(startDate: Date, days: number) {
    const result = new Array();
    startDate = new Date(startDate);
    for (let index = 0; index <= days; index++) {
      const newDay = new Date(startDate);
      newDay.setDate(newDay.getDate() + index);
      const value = this.countDurationInDay(newDay);
      const name = this.beautifyDate(newDay);
      result.push({ name, value });
    }
    return [{ name: 'minutes', series: result }];
  }

  formatSuccessData(startDate: Date, days: number) {
    const result = new Array();
    startDate = new Date(startDate);
    for (let index = 0; index <= days; index++) {
      const newDay = new Date(startDate);
      newDay.setDate(newDay.getDate() + index);
      const value = this.countBuildsByStatus(newDay, BuildStatus.Success);
      const name = this.beautifyDate(newDay);
      result.push({ name, value });
    }
    return [{ name: 'Succeed', series: result }];
  }

  countActiveProjects(){
    return [...new Set(this.user.buildHistories.map(item => item.projectId))].length;
  }

  beautifyDate(newDay: Date) {
    newDay = new Date(newDay);
    if (newDay.getMonth() + 1 < 10) {
      return newDay.getDate() + '.0' + (newDay.getMonth() + 1);
    }
    return newDay.getDate() + '.' + (newDay.getMonth() + 1);
  }

  countActiveProjectsInDay(day: Date) {
    day = new Date(day);
    return [...new Set(this.user.buildHistories.filter(x => new Date(x.buildAt).getDate() === day.getDate())
      .map(item => item.projectId))].length;
  }
  countBuildsInDay(day: Date) {
    day = new Date(day);
    return this.user.buildHistories.filter(x => new Date(x.buildAt).getDate() === day.getDate()).length;
  }

  countBuildsByStatus(day: Date, status: BuildStatus) {
    day = new Date(day);
    return this.user.buildHistories.filter(x => new Date(x.buildAt).getDate() === day.getDate()
      && x.buildStatus === status).length;
  }

  countDurationInDay(day: Date) {
    day = new Date(day);
    const builds = this.user.buildHistories.filter(x => new Date(x.buildAt).getDate() === day.getDate());
    if (builds.length > 0) {
      return builds.map(this.duration).reduce(this.sum);
    }
    return 0;
  }

  buildSucceedCount() {
    if (this.user.buildHistories.length) {
      const total = this.user.buildHistories.length;
      const successBuilds = this.user.buildHistories.filter(x => x.buildStatus === BuildStatus.Success).length;
      return Math.round((successBuilds / total * 100));
    }
    return 0;
  }
  totalBuildsCount() {
    if (this.user.buildHistories.length) {
      return this.user.buildHistories.length;
    }
    return 0;
  }

  changeMode(i: number) {
    this.tab = i;
    if (!i) {
      this.getData();
      this.month = false;
      return;
    }
    this.getData(true);
    this.month = true;
  }

  diffDates(dateOne: Date, dateTwo: Date): number {
    dateOne = new Date(dateOne);
    dateTwo = new Date(dateTwo);
    return Math.floor((dateOne.getTime() - dateTwo.getTime()) / (60 * 60 * 24 * 1000));
  }
  onlyUnique(value, index, self) {
    return self.indexOf(value) === index;
  }

  duration(history: BuildHistory) {
    return history.duration;
  }
  sum(prev, next) {
    return prev + next;
  }

  dateTickFormatting(val): string {
    if (val instanceof Date) {
      const options = { month: 'long', day: 'numeric' };
      return (val as Date).toLocaleString('en-US', options);
    }
  }

}
