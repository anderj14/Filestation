import {Routes} from '@angular/router';
import {Login} from './features/auth/login/login';
import {File} from './features/file/file';
import {Register} from './features/auth/register/register';

export const routes: Routes = [
  {path: '', redirectTo: 'file', pathMatch: 'full'},
  {path: 'file', component: File},
  {path: 'auth/login', component: Login},
  {path: 'auth/register', component: Register},
  {
    path: '**', redirectTo: '', pathMatch: 'full'
  }
];
