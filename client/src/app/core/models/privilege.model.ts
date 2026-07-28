export interface UserPrivilege {
  companyId: number;
  userId: string;
  menuName: string;
  hasRights: string | null;
  canAdd: boolean;
  canEdit: boolean;
  canDelete: boolean;
  canPost: boolean;
  canPrint: boolean;
  canView: boolean;
  canExport: boolean;
  canUnpost: boolean;
  canCheck: boolean;
}
