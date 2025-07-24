import {Component, inject} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {Authservice} from '../../../core/services/authservice';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {

  private authService = inject(Authservice);
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);

  returnUrl = '/file';
  loginError: string | null = null;

  constructor() {
    const url = this.activatedRoute.snapshot.params['returnUrl'];
    if (url) this.returnUrl = url;
  }

  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', Validators.required)
  });

  onSubmit() {
    if (this.loginForm.invalid) return;


    const { email, password } = this.loginForm.value;
    if (!email || !password) return;

    this.loginError = null;

    this.authService.login({ email, password }).subscribe({
      next: () => {
        this.authService.loadCurrentUserFromApi().subscribe({
          next: () => {
            this.router.navigateByUrl(this.returnUrl);
          },
          error: () => {
            // Si falló obtener el usuario, hacemos logout por seguridad
            this.authService.logout();
          }
        });
      },
      error: (err) => {
        this.loginError = 'Credenciales incorrectas o error en el servidor.';
      }
    });
  }
}
