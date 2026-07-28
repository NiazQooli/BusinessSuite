export interface MenuItem {
  companyId: number;
  menuName: string;
  menuSrNo: string;
  mainMenuName: string | null;
  menuCaption: string;
  hasRights: string | null;
  menuLevel: number | null;
  completeMenuName: string;
  moduleName: string;
  isReportActive: boolean;
  menuPriority: number;
  inactiveMenu: boolean;
}
