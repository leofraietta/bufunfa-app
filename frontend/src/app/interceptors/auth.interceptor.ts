import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Adiciona o token automaticamente se o usuário estiver autenticado
    let authReq = req;
    
    if (this.authService.isAuthenticated) {
      const token = this.authService.getToken();
      if (token) {
        authReq = req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`
          }
        });
      }
    }

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        // Se receber 401 (Unauthorized), redireciona para login
        if (error.status === 401) {
          console.warn('Token expirado ou inválido. Redirecionando para login.');
          this.authService.logout();
          this.router.navigate(['/login']);
        }
        
        // Se receber 403 (Forbidden), mostra mensagem de acesso negado
        if (error.status === 403) {
          console.warn('Acesso negado para este recurso.');
          // Aqui você pode mostrar uma notificação ou modal
        }

        return throwError(() => error);
      })
    );
  }
}
