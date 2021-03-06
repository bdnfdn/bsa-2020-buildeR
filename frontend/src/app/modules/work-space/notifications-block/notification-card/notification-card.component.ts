import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Notification } from '@shared/models/notification';
import { NotificationType } from '@shared/models/notification-type';
import { BuildHistoryService } from '@core/services/build-history.service';
import { Router } from '@angular/router';
import { NotificationsService } from '@core/services/notifications.service';
import { takeUntil } from 'rxjs/operators';
import { BaseComponent } from '@core/components/base/base.component';

@Component({
  selector: 'app-notification-card',
  templateUrl: './notification-card.component.html',
  styleUrls: ['./notification-card.component.sass']
})
export class NotificationCardComponent extends BaseComponent implements OnInit {
  NotificationType = NotificationType;
  @Input() notification: Notification;
  @Input() isShowingRead: boolean;
  @Output() clearedOne = new EventEmitter<Notification>();
  @Output() toggleNotificationsBlock = new EventEmitter<void>();
  @Output() read = new EventEmitter<number>();

  constructor(
    private router: Router,
    private buildHistoryService: BuildHistoryService,
    private notificationService: NotificationsService
  ) {
    super();
  }


  ngOnInit(): void {
  }

  clearOne(notification: Notification) {
    this.notificationService.markAsRead(notification.id);
    this.notification.isRead = true;
    this.read.emit(-1);
    this.clearedOne.emit(notification);
  }

  toggle() {
    this.toggleNotificationsBlock.emit();
  }

  navigateToItem(notification: Notification, event: Event) {
    if (notification.itemId) {
      event.preventDefault();
      switch (notification.type) {
        case NotificationType.Group: {
          if (notification.itemId === -1) {
            this.router.navigate(['/portal/groups']);
          }
          else {
            this.router.navigate(['/portal/groups/' + notification.itemId + '/projects']);
          }
          this.clearOne(notification);
          this.toggle();
          break;
        }
        case NotificationType.BuildCanceled:
        case NotificationType.BuildErrored:
        case NotificationType.BuildFailed:
        case NotificationType.BuildSucceeded: {
          this.buildHistoryService.getBuildHistory(notification.itemId)
            .pipe(takeUntil(this.unsubscribe$))
            .subscribe(
              bh => {
                this.router.navigate(['/portal', 'projects', bh.projectId, 'history', notification.itemId]);
                this.clearOne(notification);
              }),
            this.toggle();
          break;
        }
      }
    }
  }
}
