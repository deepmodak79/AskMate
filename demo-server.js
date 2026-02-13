const express = require('express');
const path = require('path');

const app = express();
const PORT = 3000;

// Serve static files
app.use(express.static(__dirname));

// API Mock endpoints
app.get('/api/questions', (req, res) => {
  res.json({
    items: [
      {
        id: '1',
        title: 'How to configure PRP redundancy in IEC-61850?',
        slug: 'how-to-configure-prp-redundancy-in-iec-61850',
        body: 'I need help setting up Parallel Redundancy Protocol...',
        status: 'Open',
        viewCount: 127,
        voteScore: 15,
        answerCount: 3,
        commentCount: 5,
        author: {
          id: '1',
          username: 'john_doe',
          displayName: 'John Doe',
          reputation: 2543,
          avatarUrl: null
        },
        tags: [
          { id: '1', name: 'iec-61850', usageCount: 89 },
          { id: '2', name: 'prp', usageCount: 45 },
          { id: '3', name: 'networking', usageCount: 234 }
        ],
        createdAt: new Date(Date.now() - 2 * 3600000).toISOString(),
        lastActivityAt: new Date(Date.now() - 1 * 3600000).toISOString()
      },
      {
        id: '2',
        title: 'SCADA communication timeout troubleshooting',
        slug: 'scada-communication-timeout-troubleshooting',
        body: 'Experiencing intermittent timeouts with Modbus TCP...',
        status: 'Open',
        viewCount: 45,
        voteScore: 8,
        answerCount: 1,
        commentCount: 2,
        author: {
          id: '2',
          username: 'jane_smith',
          displayName: 'Jane Smith',
          reputation: 1834,
          avatarUrl: null
        },
        tags: [
          { id: '4', name: 'scada', usageCount: 156 },
          { id: '5', name: 'modbus', usageCount: 98 },
          { id: '6', name: 'troubleshooting', usageCount: 187 }
        ],
        createdAt: new Date(Date.now() - 5 * 3600000).toISOString(),
        lastActivityAt: new Date(Date.now() - 3 * 3600000).toISOString()
      },
      {
        id: '3',
        title: 'Best practices for securing substation networks?',
        slug: 'best-practices-for-securing-substation-networks',
        body: 'What are the recommended security measures for ICS networks?',
        status: 'Open',
        viewCount: 892,
        voteScore: 23,
        answerCount: 7,
        commentCount: 12,
        author: {
          id: '3',
          username: 'security_expert',
          displayName: 'Security Expert',
          reputation: 5234,
          avatarUrl: null
        },
        tags: [
          { id: '7', name: 'security', usageCount: 312 },
          { id: '3', name: 'networking', usageCount: 234 },
          { id: '8', name: 'substation', usageCount: 145 }
        ],
        createdAt: new Date(Date.now() - 48 * 3600000).toISOString(),
        lastActivityAt: new Date(Date.now() - 12 * 3600000).toISOString()
      }
    ],
    pageNumber: 1,
    pageSize: 20,
    totalCount: 3,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false
  });
});

app.get('/api/health', (req, res) => {
  res.json({ status: 'Healthy', message: 'Deep Overflow Demo Server' });
});

app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'demo.html'));
});

app.listen(PORT, () => {
  console.log(`\n🚀 Deep Overflow Demo Server Running!\n`);
  console.log(`   ✅ Open: http://localhost:${PORT}\n`);
  console.log(`   Press Ctrl+C to stop\n`);
});
