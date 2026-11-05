import typescriptParser from '@typescript-eslint/parser';
import typescriptPlugin from '@typescript-eslint/eslint-plugin';

export default [{
  files: ['**/*.ts'],
  languageOptions: { parser: typescriptParser, parserOptions: { ecmaVersion: 'latest', sourceType: 'module' } },
  plugins: { '@typescript-eslint': typescriptPlugin },
  rules: {
    ...typescriptPlugin.configs.recommended.rules,
    '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_', caughtErrorsIgnorePattern: '^_' }],
  },
}];
