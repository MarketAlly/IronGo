package types

// Basic type declarations
type ID int64
type Name string

// Struct types
type Person struct {
	ID       ID     `json:"id"`
	Name     Name   `json:"name"`
	Age      int    `json:"age"`
	Email    string `json:"email,omitempty"`
	Address  *Address
	Tags     []string
	Metadata map[string]interface{}
}

type Address struct {
	Street  string
	City    string
	State   string
	ZipCode string
}

// Interface types
type Writer interface {
	Write([]byte) (int, error)
}

type Reader interface {
	Read([]byte) (int, error)
}

type ReadWriter interface {
	Reader
	Writer
}

type Closer interface {
	Close() error
}

type ReadWriteCloser interface {
	ReadWriter
	Closer
}

// Function types
type Handler func(w ResponseWriter, r *Request)
type Middleware func(Handler) Handler
type ErrorHandler func(error) error

// Generic types (Go 1.18+)
type List[T any] struct {
	items []T
}

type Map[K comparable, V any] struct {
	m map[K]V
}

type Result[T any] struct {
	Value T
	Error error
}

// Methods
func (p *Person) FullName() string {
	return string(p.Name)
}

func (p *Person) IsAdult() bool {
	return p.Age >= 18
}

func (l *List[T]) Add(item T) {
	l.items = append(l.items, item)
}

func (l *List[T]) Get(index int) (T, bool) {
	var zero T
	if index < 0 || index >= len(l.items) {
		return zero, false
	}
	return l.items[index], true
}