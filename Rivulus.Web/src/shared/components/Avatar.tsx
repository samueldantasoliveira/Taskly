import { cn } from '../lib/cn'
import { avatarOptions, avatarPath, isAvatarKey, type AvatarKey } from '../lib/avatars'

function initials(name: string) {
  return name
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('')
}

export function Avatar({ name, avatarKey, size = 'md' }: { name: string; avatarKey?: string | null; size?: 'sm' | 'md' | 'lg' }) {
  const selectedAvatar = isAvatarKey(avatarKey) ? avatarKey : null
  return (
    <span className={cn('avatar', `avatar--${size}`)} aria-label={name}>
      {selectedAvatar
        ? <img src={avatarPath(selectedAvatar)} alt="" />
        : initials(name)}
    </span>
  )
}

export function AvatarPicker({ value, onChange }: { value?: string | null; onChange: (key: AvatarKey) => void }) {
  return (
    <fieldset className="avatar-picker">
      <legend>Escolha seu companheiro de corrente</legend>
      <div className="avatar-picker__grid">
        {avatarOptions.map((avatar) => (
          <label key={avatar.key} className={cn('avatar-option', value === avatar.key && 'avatar-option--selected')}>
            <input type="radio" name="avatar" value={avatar.key} checked={value === avatar.key} onChange={() => onChange(avatar.key)} />
            <img src={avatarPath(avatar.key)} alt="" />
            <span>{avatar.label}</span>
          </label>
        ))}
      </div>
    </fieldset>
  )
}
