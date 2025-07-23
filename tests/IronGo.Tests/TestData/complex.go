package main

import (
	"context"
	"errors"
	"fmt"
	"log"
	"sync"
	"time"
)

// Constants
const (
	DefaultTimeout = 30 * time.Second
	MaxRetries     = 3
	BufferSize     = 1024
)

// Global variables
var (
	ErrNotFound = errors.New("not found")
	ErrTimeout  = errors.New("timeout")
	
	globalMutex sync.RWMutex
	cache       = make(map[string]interface{})
)

// Complex struct with embedded types
type Server struct {
	sync.RWMutex
	name      string
	address   string
	clients   map[string]*Client
	started   time.Time
	config    *Config
	ctx       context.Context
	cancel    context.CancelFunc
	waitGroup sync.WaitGroup
}

type Client struct {
	ID       string
	Name     string
	conn     interface{}
	messages chan Message
	done     chan struct{}
}

type Message struct {
	Type    MessageType
	Payload []byte
	From    string
	To      string
	Time    time.Time
}

type MessageType int

const (
	MessageTypeText MessageType = iota
	MessageTypeBinary
	MessageTypeControl
)

type Config struct {
	Port            int
	MaxConnections  int
	ReadTimeout     time.Duration
	WriteTimeout    time.Duration
	TLSEnabled      bool
	TLSCertFile     string
	TLSKeyFile      string
	AllowedOrigins  []string
	RateLimitPerSec int
}

// Interface with multiple methods
type Handler interface {
	Handle(context.Context, *Request) (*Response, error)
	Validate(*Request) error
	Name() string
}

type Request struct {
	Method  string
	Path    string
	Headers map[string][]string
	Body    []byte
}

type Response struct {
	StatusCode int
	Headers    map[string][]string
	Body       []byte
}

// Complex function with multiple features
func (s *Server) Start() error {
	s.Lock()
	defer s.Unlock()
	
	if s.started.IsZero() {
		s.started = time.Now()
		log.Printf("Server %s starting at %s", s.name, s.address)
		
		// Initialize with context
		s.ctx, s.cancel = context.WithCancel(context.Background())
		
		// Start background workers
		for i := 0; i < s.config.MaxConnections; i++ {
			s.waitGroup.Add(1)
			go s.worker(i)
		}
		
		// Start listener
		s.waitGroup.Add(1)
		go s.listen()
		
		return nil
	}
	
	return errors.New("server already started")
}

// Method with goroutines and channels
func (s *Server) worker(id int) {
	defer s.waitGroup.Done()
	
	ticker := time.NewTicker(time.Second)
	defer ticker.Stop()
	
	for {
		select {
		case <-s.ctx.Done():
			log.Printf("Worker %d shutting down", id)
			return
			
		case <-ticker.C:
			// Periodic cleanup
			s.cleanup()
			
		default:
			// Process messages
			if client := s.getNextClient(); client != nil {
				s.processClient(client)
			} else {
				time.Sleep(10 * time.Millisecond)
			}
		}
	}
}

// Method with error handling and defer
func (s *Server) processClient(c *Client) {
	defer func() {
		if r := recover(); r != nil {
			log.Printf("Panic in processClient: %v", r)
		}
	}()
	
	timeout := time.NewTimer(s.config.ReadTimeout)
	defer timeout.Stop()
	
	select {
	case msg := <-c.messages:
		if err := s.handleMessage(c, msg); err != nil {
			log.Printf("Error handling message: %v", err)
			s.disconnectClient(c)
		}
		
	case <-timeout.C:
		log.Printf("Client %s timed out", c.ID)
		s.disconnectClient(c)
		
	case <-c.done:
		s.disconnectClient(c)
	}
}

// Generic function
func Map[T, U any](items []T, fn func(T) U) []U {
	result := make([]U, len(items))
	for i, item := range items {
		result[i] = fn(item)
	}
	return result
}

// Function with type constraints
func Sum[T ~int | ~int64 | ~float64](values []T) T {
	var sum T
	for _, v := range values {
		sum += v
	}
	return sum
}

// Variadic function
func Printf(format string, args ...interface{}) {
	fmt.Printf(format, args...)
}

// Function returning multiple values and error
func ParseConfig(data []byte) (*Config, error) {
	if len(data) == 0 {
		return nil, errors.New("empty config data")
	}
	
	config := &Config{
		Port:            8080,
		MaxConnections:  100,
		ReadTimeout:     30 * time.Second,
		WriteTimeout:    30 * time.Second,
		RateLimitPerSec: 100,
	}
	
	// Parse config...
	
	return config, nil
}

// Complex control flow
func ProcessData(data []byte) error {
	if len(data) == 0 {
		return errors.New("empty data")
	}
	
	// Switch statement
	switch data[0] {
	case 0x00:
		return processText(data[1:])
	case 0x01:
		return processBinary(data[1:])
	case 0x02, 0x03:
		return processControl(data[1:])
	default:
		return fmt.Errorf("unknown data type: %x", data[0])
	}
}

// Range loops
func (s *Server) cleanup() {
	s.RLock()
	clients := make([]*Client, 0, len(s.clients))
	for _, client := range s.clients {
		clients = append(clients, client)
	}
	s.RUnlock()
	
	for i, client := range clients {
		if !s.isClientActive(client) {
			log.Printf("Removing inactive client %d: %s", i, client.ID)
			s.disconnectClient(client)
		}
	}
}

// Type assertion and type switch
func ConvertValue(v interface{}) string {
	switch val := v.(type) {
	case string:
		return val
	case int:
		return fmt.Sprintf("%d", val)
	case float64:
		return fmt.Sprintf("%.2f", val)
	case bool:
		if val {
			return "true"
		}
		return "false"
	case fmt.Stringer:
		return val.String()
	default:
		return fmt.Sprintf("%v", val)
	}
}

// Init function
func init() {
	log.SetPrefix("[Server] ")
	log.SetFlags(log.Ldate | log.Ltime | log.Lmicroseconds)
}

// Helper functions
func (s *Server) getNextClient() *Client {
	s.RLock()
	defer s.RUnlock()
	
	for _, client := range s.clients {
		select {
		case <-client.messages:
			return client
		default:
			continue
		}
	}
	return nil
}

func (s *Server) isClientActive(c *Client) bool {
	select {
	case <-c.done:
		return false
	default:
		return true
	}
}

func (s *Server) disconnectClient(c *Client) {
	s.Lock()
	defer s.Unlock()
	
	if _, exists := s.clients[c.ID]; exists {
		delete(s.clients, c.ID)
		close(c.done)
		log.Printf("Client %s disconnected", c.ID)
	}
}

func (s *Server) handleMessage(c *Client, msg Message) error {
	// Message handling logic
	return nil
}

func (s *Server) listen() {
	defer s.waitGroup.Done()
	// Listener implementation
}

func processText(data []byte) error {
	// Process text data
	return nil
}

func processBinary(data []byte) error {
	// Process binary data
	return nil
}

func processControl(data []byte) error {
	// Process control data
	return nil
}