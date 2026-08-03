// User API contracts used by personal and administrator profile workflows.
export interface UserProfile {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  imageId: string | null;
}

export interface UpdateUserProfileRequest {
  firstName?: string | null;
  lastName?: string | null;
  imageId?: string | null;
}
