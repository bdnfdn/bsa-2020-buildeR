import { HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { HttpService } from '../../core/services/http.service';
import { User } from '../../shared/models/user/user';
import { NewUser } from '../../shared/models/user/new-user';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  public routePrefix = '/users';

  constructor(private httpService: HttpService) { }

  getUser() {
    return this.httpService.getFullRequest<User>('template');
  }
  getCurrentUser() {
    return this.httpService.getRequest<User>(environment.apiUrl + '/auth/');
  }

  createUser(user: NewUser): Observable<HttpResponse<User>> {
    return this.httpService.postFullRequest<User>(`${this.routePrefix}`, user);
  }

  getUserById(userId: number): Observable<HttpResponse<User>> {
    return this.httpService.getFullRequest<User>(`${this.routePrefix}/${userId}`);
  }

  getUserByUId(uniqueId: string): Observable<HttpResponse<User>> {
    return this.httpService.getFullRequest<User>(`${this.routePrefix}/getbyuid/${uniqueId}`);
  }

}
