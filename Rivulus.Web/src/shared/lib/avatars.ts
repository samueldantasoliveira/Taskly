export const avatarOptions = [
  { key: 'capybara', label: 'Capivara' },
  { key: 'otter', label: 'Lontra' },
  { key: 'duck', label: 'Pato' },
  { key: 'turtle', label: 'Tartaruga' },
  { key: 'fish', label: 'Peixe' },
  { key: 'kingfisher', label: 'Martim-pescador' },
] as const

export type AvatarKey = (typeof avatarOptions)[number]['key']

const avatarKeys = new Set<string>(avatarOptions.map((avatar) => avatar.key))

export function isAvatarKey(value: string | null | undefined): value is AvatarKey {
  return Boolean(value && avatarKeys.has(value))
}

export function avatarPath(key: AvatarKey) {
  return `/avatars/${key}.svg`
}
