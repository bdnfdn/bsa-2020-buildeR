import { Providers } from '../providers';

export interface NewUser {
  role: string;
  email: string;
  username: string;
  firstName: string;
  lastName: string;
  avatarUrl: string;
  uId: string;
  providerId: Providers;
  providerUrl: string;
  accessToken: string;
}
