// Authentication account contracts used by the admin portal.
export interface Permission {
  id: number;
  name: string;
}

export interface Role {
  id: number;
  name: string;
  permissions: Permission[];
}

export interface AccountUser {
  id: string;
  fullName: string;
  email: string;
  found: boolean;
}

export interface Account {
  id: string;
  email: string;
  identityId: string;
  userId: string | null;
  isActive: boolean;
  createdAtUtc: string;
  deletedAtUtc: string | null;
  roles: Role[];
  user: AccountUser | null;
}
