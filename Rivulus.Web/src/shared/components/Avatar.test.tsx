import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { Avatar, AvatarPicker } from './Avatar'

describe('Avatar', () => {
  it('renders a selected animal and falls back to initials for an unknown key', () => {
    const { rerender } = render(<Avatar name="Samuel Oliveira" avatarKey="otter" />)

    expect(screen.getByLabelText('Samuel Oliveira').querySelector('img'))
      .toHaveAttribute('src', '/avatars/otter.svg')

    rerender(<Avatar name="Samuel Oliveira" avatarKey="unknown" />)
    expect(screen.getByLabelText('Samuel Oliveira')).toHaveTextContent('SO')
  })

  it('lets the user select one of the available animals', async () => {
    const onChange = vi.fn()
    render(<AvatarPicker value="capybara" onChange={onChange} />)

    await userEvent.click(screen.getByRole('radio', { name: 'Lontra' }))

    expect(onChange).toHaveBeenCalledWith('otter')
  })
})
