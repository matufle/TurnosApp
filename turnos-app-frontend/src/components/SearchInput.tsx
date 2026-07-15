// src/components/SearchInput.tsx
import { TextInput } from '@mantine/core';
import type { TextInputProps } from '@mantine/core';
import { IconSearch } from '@tabler/icons-react';

interface SearchInputProps extends TextInputProps {
  onSearch: (value: string) => void;
}

export function SearchInput({ onSearch, ...props }: SearchInputProps) {
  return (
    <TextInput
      placeholder="Buscar..."
      leftSection={<IconSearch size={16} />}
      onChange={(event) => onSearch(event.currentTarget.value)}
      {...props}
    />
  );
}