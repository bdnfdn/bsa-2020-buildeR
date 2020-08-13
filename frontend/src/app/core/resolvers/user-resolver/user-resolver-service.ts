import { Injectable } from '@angular/core';
import { Router, RouterStateSnapshot, ActivatedRouteSnapshot, Resolve } from '@angular/router';
import { User } from '@shared/models/user';
import { tap, map, catchError } from 'rxjs/operators/';
import { EMPTY } from 'rxjs';
import {UserService} from "../../services/user.service";
import {AuthenticationService} from "@core/services/authentication.service";

@Injectable({
  providedIn: 'root'
})
export class UserResolverService implements Resolve<User>{

  constructor(private router: Router, private userService: UserService, private authService: AuthenticationService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot){
    return this.userService.getUserById(this.authService.getUser().id).pipe(tap((resp) => {
      if (resp){
        return resp;
      }
      else{
        return EMPTY;
      }
      // accoridng to serer logic in case of null we getting exception
    }), catchError(() => {
      console.log(this.router.url);
      this.router.navigateByUrl('/portal/**', { skipLocationChange: true });
      return EMPTY;
    }));
  }
}