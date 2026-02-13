import http.server
import socketserver
import json
from datetime import datetime, timedelta

PORT = 3000

class DeepOverflowHandler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        if self.path == '/api/questions':
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()
            
            questions = {
                "items": [
                    {
                        "id": "1",
                        "title": "How to configure PRP redundancy in IEC-61850?",
                        "slug": "how-to-configure-prp-redundancy-in-iec-61850",
                        "body": "I need help setting up Parallel Redundancy Protocol in our substation automation system...",
                        "status": "Open",
                        "viewCount": 127,
                        "voteScore": 15,
                        "answerCount": 3,
                        "commentCount": 5,
                        "author": {
                            "id": "1",
                            "username": "john_doe",
                            "displayName": "John Doe",
                            "reputation": 2543
                        },
                        "tags": [
                            {"id": "1", "name": "iec-61850", "usageCount": 89},
                            {"id": "2", "name": "prp", "usageCount": 45},
                            {"id": "3", "name": "networking", "usageCount": 234}
                        ],
                        "createdAt": (datetime.now() - timedelta(hours=2)).isoformat(),
                        "lastActivityAt": (datetime.now() - timedelta(hours=1)).isoformat()
                    },
                    {
                        "id": "2",
                        "title": "SCADA communication timeout troubleshooting",
                        "slug": "scada-communication-timeout-troubleshooting",
                        "body": "Experiencing intermittent timeouts with Modbus TCP connections...",
                        "status": "Open",
                        "viewCount": 45,
                        "voteScore": 8,
                        "answerCount": 1,
                        "commentCount": 2,
                        "author": {
                            "id": "2",
                            "username": "jane_smith",
                            "displayName": "Jane Smith",
                            "reputation": 1834
                        },
                        "tags": [
                            {"id": "4", "name": "scada", "usageCount": 156},
                            {"id": "5", "name": "modbus", "usageCount": 98}
                        ],
                        "createdAt": (datetime.now() - timedelta(hours=5)).isoformat(),
                        "lastActivityAt": (datetime.now() - timedelta(hours=3)).isoformat()
                    },
                    {
                        "id": "3",
                        "title": "Best practices for securing substation networks?",
                        "slug": "best-practices-for-securing-substation-networks",
                        "body": "What are the recommended security measures for ICS networks in substations?",
                        "status": "Open",
                        "viewCount": 892,
                        "voteScore": 23,
                        "answerCount": 7,
                        "commentCount": 12,
                        "author": {
                            "id": "3",
                            "username": "security_expert",
                            "displayName": "Security Expert",
                            "reputation": 5234
                        },
                        "tags": [
                            {"id": "7", "name": "security", "usageCount": 312},
                            {"id": "3", "name": "networking", "usageCount": 234}
                        ],
                        "createdAt": (datetime.now() - timedelta(days=2)).isoformat(),
                        "lastActivityAt": (datetime.now() - timedelta(hours=12)).isoformat()
                    }
                ],
                "pageNumber": 1,
                "pageSize": 20,
                "totalCount": 3,
                "totalPages": 1
            }
            
            self.wfile.write(json.dumps(questions).encode())
            
        elif self.path == '/api/health':
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({"status": "Healthy", "message": "Deep Overflow Demo"}).encode())
            
        else:
            # Serve demo.html for all other paths
            self.path = '/demo.html'
            return http.server.SimpleHTTPRequestHandler.do_GET(self)

print("\n" + "="*60)
print("🚀 Deep Overflow Demo Server")
print("="*60)
print(f"\n   ✅ Running at: http://localhost:{PORT}")
print(f"   📱 Open your browser and visit the link above!")
print(f"\n   Press Ctrl+C to stop the server\n")
print("="*60 + "\n")

with socketserver.TCPServer(("", PORT), DeepOverflowHandler) as httpd:
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\n\n👋 Server stopped!")
