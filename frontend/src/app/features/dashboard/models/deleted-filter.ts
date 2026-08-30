import { HttpParams } from '@angular/common/http';

export function withIsDeletedParam(isDeleted = false): { params: HttpParams } {
  return {
    params: new HttpParams().set('isDeleted', String(isDeleted))
  };
}
