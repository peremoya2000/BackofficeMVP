import { HttpErrorResponse, HttpInterceptorFn, HttpStatusCode } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { ProblemDetails } from './problem-details';

const SNACK_BAR_DURATION_MS = 6000;

export const httpErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const snackBar = inject(MatSnackBar);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        snackBar.open(toUserMessage(error), 'Cerrar', { duration: SNACK_BAR_DURATION_MS });
      }
      return throwError(() => error);
    }),
  );
};

function toUserMessage(error: HttpErrorResponse): string {
  const problem = error.error as ProblemDetails | null;
  // Only domain errors (they carry a code) have a Spanish detail; framework errors are in English.
  if (problem?.code && problem.detail) return problem.detail;

  switch (error.status) {
    case 0:
    case HttpStatusCode.BadGateway:
    case HttpStatusCode.ServiceUnavailable:
    case HttpStatusCode.GatewayTimeout:
      return 'No se puede conectar con el servidor. Inténtalo de nuevo en unos segundos.';
    case HttpStatusCode.BadRequest:
      return 'La solicitud no es válida. Revisa los datos introducidos.';
    case HttpStatusCode.NotFound:
      return 'No se ha encontrado el recurso solicitado.';
    case HttpStatusCode.Conflict:
      return 'Los datos han cambiado mientras trabajabas. Recarga e inténtalo de nuevo.';
    default:
      return 'Se ha producido un error inesperado. Inténtalo de nuevo más tarde.';
  }
}
