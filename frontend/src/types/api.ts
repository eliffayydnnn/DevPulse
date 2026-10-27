export interface ApiResponse<T> {
  success: boolean;
  data: T;
  errorMessage?: string;
  details?: unknown;
  timestampUtc: string;
}

export interface GitHubRepositoryDto {
  id: number;
  name: string;
  fullName: string;
  owner: string;
  ownerAvatarUrl?: string;
  description?: string;
  htmlUrl: string;
  stars: number;
  forks: number;
  openIssues: number;
  watchers: number;
  primaryLanguage?: string;
  defaultBranch?: string;
  license?: string;
  isArchived: boolean;
  isFork: boolean;
  createdAt: string;
  updatedAt: string;
  pushedAt?: string;
}

export interface GitHubUserDto {
  login: string;
  name?: string;
  avatarUrl?: string;
  bio?: string;
  company?: string;
  location?: string;
  blog?: string;
  twitterUsername?: string;
  publicRepos: number;
  followers: number;
  following: number;
  htmlUrl: string;
  createdAt: string;
}

export interface CommitSummaryDto {
  sha: string;
  shortSha: string;
  message: string;
  authorName: string;
  authorEmail?: string;
  authorAvatarUrl?: string;
  date: string;
  htmlUrl: string;
}

export interface PullRequestSummaryDto {
  number: number;
  title: string;
  state: string;
  isMerged: boolean;
  isDraft: boolean;
  authorLogin: string;
  authorAvatarUrl?: string;
  createdAt: string;
  updatedAt: string;
  closedAt?: string;
  mergedAt?: string;
  htmlUrl: string;
}

export interface IssueSummaryDto {
  number: number;
  title: string;
  state: string;
  authorLogin: string;
  authorAvatarUrl?: string;
  commentsCount: number;
  labels: string[];
  createdAt: string;
  updatedAt: string;
  closedAt?: string;
  htmlUrl: string;
}

export interface LanguageDistributionDto {
  language: string;
  bytes: number;
  percentage: number;
  colorHex?: string;
}

export interface ActivityDataPointDto {
  periodLabel: string;
  periodStart: string;
  commits: number;
  pullRequests: number;
  issues: number;
}

export interface RepositoryMetricsDto {
  stars: number;
  forks: number;
  openIssues: number;
  totalRecentCommitsEvaluated: number;
  openPullRequests: number;
  closedPullRequests: number;
  openIssuesCount: number;
  closedIssuesCount: number;
  healthScore: number;
  healthGrade: string;
  daysSinceLastCommit: number;
  pullRequestMergeRatio: number;
  activityStatus: string;
}

export interface RepositoryAnalysisDto {
  repository: GitHubRepositoryDto;
  metrics: RepositoryMetricsDto;
  recentCommits: CommitSummaryDto[];
  pullRequests: PullRequestSummaryDto[];
  issues: IssueSummaryDto[];
  languages: LanguageDistributionDto[];
  activityTrend: ActivityDataPointDto[];
  analyzedAtUtc: string;
  isFromCache: boolean;
}

export interface MetricComparisonRowDto {
  metricKey: string;
  metricLabel: string;
  category: string;
  formattedValues: Record<string, string>;
  winnerRepositoryFullName?: string;
}

export interface RepositoryComparisonDto {
  repositories: RepositoryAnalysisDto[];
  comparisonMatrix: MetricComparisonRowDto[];
  overallVerdict: string;
  comparedAtUtc: string;
}

export interface RateLimitInfoDto {
  limit: number;
  remaining: number;
  resetTimeUtc?: string;
  used: number;
  percentageRemaining: number;
}
