import {computed, inject, Injectable, signal} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Router} from '@angular/router';
import {environment} from '../../../environments/environment.development';
import {User} from '../../shared/model/user';
import {tap} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Authservice {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = environment.apiUrl;

  private currentUserSignal = signal<User | null>(null);
  currentUser = computed(() => this.currentUserSignal());

  // === TOKEN ===
  private getToken(): string | null {
    return localStorage.getItem('token');
  }

  private setToken(token: string) {
    localStorage.setItem('token', token);
  }

  private clearToken() {
    localStorage.removeItem('token');
  }


  login(values: { email: string; password: string }) {
    return this.http.post<{ token: string }>(`${this.baseUrl}auth/login`, values).pipe(
      tap(res => {
        this.setToken(res.token);
      })
    );
  }

  register(data: { username: string; email: string; password: string }) {
    return this.http.post(`${this.baseUrl}auth/register`, data);
  }

  logout() {
    this.clearToken();
    this.currentUserSignal.set(null);
    this.router.navigateByUrl('/auth/login');
  }

  loadCurrentUserFromApi() {
    return this.http.get<User>(`${this.baseUrl}auth/current-user`).pipe(
      tap(user => this.currentUserSignal.set(user))
    );
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

}
