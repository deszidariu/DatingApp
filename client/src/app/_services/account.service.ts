import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
@Injectable({
  providedIn: 'root'
})
export class AccountService {

  baseUrl = environment.apiUrl;
  private numberOfPreviousValuesToStore = 1;
  private currentuserSource = new ReplaySubject<User>(this.numberOfPreviousValuesToStore);
  currentuser$ = this.currentuserSource.asObservable();
  constructor(private http: HttpClient) { }

  login(model: any){
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {
        const user = response;
        if(user){
          console.log(user.photoUrl);
          this.settCurrentUser(user);
        }
        return user;
      })
    );
  }

  register(model: any){
    return this.http.post(this.baseUrl + "account/register", model).pipe(
      map((user: User) =>{
        if(user){
          this.settCurrentUser(user);
        }
      }  
      )
    )
  }

  settCurrentUser(user: User){
    localStorage.setItem("user", JSON.stringify(user));
    this.currentuserSource.next(user);
  }

  logout() {
    localStorage.removeItem('user'); 
    this.currentuserSource.next(null);
  }
}
