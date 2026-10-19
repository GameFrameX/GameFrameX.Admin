import DOMPurify from 'dompurify'

export function sanitizeHtml(html: string): string {
	return DOMPurify.sanitize(html, {
		ALLOWED_TAGS: ['p', 'br', 'b', 'i', 'em', 'strong', 'a', 'ul', 'ol', 'li', 'span', 'div', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'img', 'table', 'tr', 'td', 'th', 'blockquote', 'pre', 'code'],
		ALLOWED_ATTR: ['href', 'src', 'alt', 'title', 'target', 'class', 'style'],
	})
}
