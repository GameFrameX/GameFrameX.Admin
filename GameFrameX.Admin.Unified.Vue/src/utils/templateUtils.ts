import Handlebars from 'handlebars'

export function extractVariables(text: string): string[] {
  if (!text) return []
  const matches = text.matchAll(/{{(.+?)}}/g)
  return [...new Set([...matches].map(m => m[1].trim()))]
}

export function extractVariablesFromAll(...texts: (string | undefined | null)[]): string[] {
  const allVars: string[] = []
  for (const text of texts) {
    allVars.push(...extractVariables(text ?? ''))
  }
  return [...new Set(allVars)]
}

export function renderTemplate(template: string, data: Record<string, string>): string {
  if (!template) return ''
  return Handlebars.compile(template)(data)
}

export function toVariableLabels(variables: string[]): string[] {
  return variables.map(v => `{{${v}}}`)
}
