-- Deep Overflow Database Schema
-- PostgreSQL 15+
-- Production-ready schema with proper indexing, constraints, and audit support

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- For full-text search optimization

-- ============================================================================
-- USERS & AUTHENTICATION
-- ============================================================================

CREATE TYPE user_role AS ENUM ('User', 'Moderator', 'Admin');
CREATE TYPE auth_provider AS ENUM ('SSO', 'Email', 'OAuth');

CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,
    display_name VARCHAR(255) NOT NULL,
    password_hash VARCHAR(500), -- For email-based auth
    auth_provider auth_provider NOT NULL DEFAULT 'Email',
    sso_id VARCHAR(255) UNIQUE,
    
    -- Profile
    department VARCHAR(100),
    job_title VARCHAR(100),
    bio TEXT,
    avatar_url VARCHAR(500),
    location VARCHAR(100),
    skills TEXT[], -- Array of skills
    
    -- Status
    role user_role NOT NULL DEFAULT 'User',
    reputation INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_email_verified BOOLEAN NOT NULL DEFAULT false,
    
    -- Security
    last_login_at TIMESTAMP WITH TIME ZONE,
    last_login_ip INET,
    failed_login_attempts INTEGER NOT NULL DEFAULT 0,
    locked_until TIMESTAMP WITH TIME ZONE,
    password_changed_at TIMESTAMP WITH TIME ZONE,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT valid_email CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    CONSTRAINT valid_reputation CHECK (reputation >= 0)
);

CREATE INDEX idx_users_email ON users(email) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_username ON users(username) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_reputation ON users(reputation DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_department ON users(department) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_role ON users(role);

-- ============================================================================
-- TAGS
-- ============================================================================

CREATE TYPE tag_category AS ENUM ('Hardware', 'Software', 'Protocol', 'General', 'Network', 'DevOps');

CREATE TABLE tags (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(50) UNIQUE NOT NULL,
    description TEXT,
    category tag_category NOT NULL DEFAULT 'General',
    usage_count INTEGER NOT NULL DEFAULT 0,
    created_by UUID NOT NULL REFERENCES users(id),
    is_approved BOOLEAN NOT NULL DEFAULT false,
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT valid_tag_name CHECK (name ~* '^[a-z0-9\-]+$')
);

CREATE INDEX idx_tags_name ON tags(name);
CREATE INDEX idx_tags_category ON tags(category);
CREATE INDEX idx_tags_usage_count ON tags(usage_count DESC);
CREATE INDEX idx_tags_name_trgm ON tags USING gin(name gin_trgm_ops);

-- ============================================================================
-- QUESTIONS
-- ============================================================================

CREATE TYPE question_status AS ENUM ('Open', 'Closed', 'Duplicate', 'OffTopic', 'Resolved');

CREATE TABLE questions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(300) NOT NULL,
    slug VARCHAR(350) UNIQUE NOT NULL,
    body TEXT NOT NULL,
    
    -- Ownership
    author_id UUID NOT NULL REFERENCES users(id),
    
    -- Status
    status question_status NOT NULL DEFAULT 'Open',
    accepted_answer_id UUID, -- Self-referential, set later
    duplicate_of_question_id UUID REFERENCES questions(id),
    
    -- Engagement
    view_count INTEGER NOT NULL DEFAULT 0,
    vote_score INTEGER NOT NULL DEFAULT 0,
    answer_count INTEGER NOT NULL DEFAULT 0,
    comment_count INTEGER NOT NULL DEFAULT 0,
    bookmark_count INTEGER NOT NULL DEFAULT 0,
    
    -- Moderation
    is_locked BOOLEAN NOT NULL DEFAULT false,
    locked_by UUID REFERENCES users(id),
    locked_at TIMESTAMP WITH TIME ZONE,
    lock_reason TEXT,
    
    is_featured BOOLEAN NOT NULL DEFAULT false,
    featured_until TIMESTAMP WITH TIME ZONE,
    
    -- Timestamps
    last_activity_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT valid_title_length CHECK (char_length(title) >= 10),
    CONSTRAINT valid_body_length CHECK (char_length(body) >= 20)
);

CREATE INDEX idx_questions_author ON questions(author_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_questions_status ON questions(status) WHERE deleted_at IS NULL;
CREATE INDEX idx_questions_created_at ON questions(created_at DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_questions_last_activity ON questions(last_activity_at DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_questions_vote_score ON questions(vote_score DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_questions_view_count ON questions(view_count DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_questions_slug ON questions(slug) WHERE deleted_at IS NULL;
CREATE INDEX idx_questions_title_trgm ON questions USING gin(title gin_trgm_ops);
CREATE INDEX idx_questions_body_trgm ON questions USING gin(body gin_trgm_ops);

-- ============================================================================
-- QUESTION TAGS (Many-to-Many)
-- ============================================================================

CREATE TABLE question_tags (
    question_id UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    tag_id UUID NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    PRIMARY KEY (question_id, tag_id)
);

CREATE INDEX idx_question_tags_tag ON question_tags(tag_id);
CREATE INDEX idx_question_tags_question ON question_tags(question_id);

-- ============================================================================
-- ANSWERS
-- ============================================================================

CREATE TABLE answers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    question_id UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    body TEXT NOT NULL,
    
    -- Ownership
    author_id UUID NOT NULL REFERENCES users(id),
    
    -- Status
    is_accepted BOOLEAN NOT NULL DEFAULT false,
    accepted_at TIMESTAMP WITH TIME ZONE,
    
    -- Engagement
    vote_score INTEGER NOT NULL DEFAULT 0,
    comment_count INTEGER NOT NULL DEFAULT 0,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT valid_answer_length CHECK (char_length(body) >= 20)
);

CREATE INDEX idx_answers_question ON answers(question_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_answers_author ON answers(author_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_answers_created_at ON answers(created_at DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_answers_vote_score ON answers(vote_score DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_answers_is_accepted ON answers(is_accepted) WHERE deleted_at IS NULL;

-- Now add the foreign key for accepted_answer_id
ALTER TABLE questions ADD CONSTRAINT fk_questions_accepted_answer 
    FOREIGN KEY (accepted_answer_id) REFERENCES answers(id);

-- ============================================================================
-- COMMENTS
-- ============================================================================

CREATE TYPE comment_target_type AS ENUM ('Question', 'Answer');

CREATE TABLE comments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    body TEXT NOT NULL,
    
    -- Polymorphic relationship
    target_type comment_target_type NOT NULL,
    target_id UUID NOT NULL,
    
    -- Ownership
    author_id UUID NOT NULL REFERENCES users(id),
    
    -- Engagement
    vote_score INTEGER NOT NULL DEFAULT 0,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT valid_comment_length CHECK (char_length(body) >= 5 AND char_length(body) <= 600)
);

CREATE INDEX idx_comments_target ON comments(target_type, target_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_comments_author ON comments(author_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_comments_created_at ON comments(created_at ASC);

-- ============================================================================
-- VOTES
-- ============================================================================

CREATE TYPE vote_type AS ENUM ('Upvote', 'Downvote');
CREATE TYPE vote_target_type AS ENUM ('Question', 'Answer', 'Comment');

CREATE TABLE votes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Polymorphic relationship
    target_type vote_target_type NOT NULL,
    target_id UUID NOT NULL,
    
    -- Vote details
    vote_type vote_type NOT NULL,
    user_id UUID NOT NULL REFERENCES users(id),
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT unique_vote_per_user UNIQUE (user_id, target_type, target_id)
);

CREATE INDEX idx_votes_target ON votes(target_type, target_id);
CREATE INDEX idx_votes_user ON votes(user_id);

-- ============================================================================
-- BOOKMARKS
-- ============================================================================

CREATE TABLE bookmarks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    question_id UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    notes TEXT,
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT unique_bookmark UNIQUE (user_id, question_id)
);

CREATE INDEX idx_bookmarks_user ON bookmarks(user_id);
CREATE INDEX idx_bookmarks_question ON bookmarks(question_id);

-- ============================================================================
-- EDIT HISTORY
-- ============================================================================

CREATE TYPE edit_target_type AS ENUM ('Question', 'Answer', 'Tag');

CREATE TABLE edit_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Polymorphic relationship
    target_type edit_target_type NOT NULL,
    target_id UUID NOT NULL,
    
    -- Edit details
    editor_id UUID NOT NULL REFERENCES users(id),
    edit_summary VARCHAR(300),
    
    -- Content snapshots
    before_title VARCHAR(300),
    after_title VARCHAR(300),
    before_body TEXT,
    after_body TEXT,
    before_tags UUID[],
    after_tags UUID[],
    
    -- Approval (for low-rep users)
    needs_approval BOOLEAN NOT NULL DEFAULT false,
    is_approved BOOLEAN,
    reviewed_by UUID REFERENCES users(id),
    reviewed_at TIMESTAMP WITH TIME ZONE,
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_edit_history_target ON edit_history(target_type, target_id);
CREATE INDEX idx_edit_history_editor ON edit_history(editor_id);
CREATE INDEX idx_edit_history_created_at ON edit_history(created_at DESC);

-- ============================================================================
-- FLAGS & MODERATION
-- ============================================================================

CREATE TYPE flag_reason AS ENUM (
    'Spam', 
    'Offensive', 
    'LowQuality', 
    'NeedsModeratorAttention', 
    'Duplicate',
    'OffTopic',
    'NotAnAnswer'
);

CREATE TYPE flag_target_type AS ENUM ('Question', 'Answer', 'Comment');
CREATE TYPE flag_status AS ENUM ('Pending', 'Reviewed', 'Declined', 'Helpful');

CREATE TABLE flags (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Polymorphic relationship
    target_type flag_target_type NOT NULL,
    target_id UUID NOT NULL,
    
    -- Flag details
    reason flag_reason NOT NULL,
    description TEXT,
    flagger_id UUID NOT NULL REFERENCES users(id),
    
    -- Resolution
    status flag_status NOT NULL DEFAULT 'Pending',
    reviewed_by UUID REFERENCES users(id),
    reviewed_at TIMESTAMP WITH TIME ZONE,
    moderator_note TEXT,
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_flags_target ON flags(target_type, target_id);
CREATE INDEX idx_flags_status ON flags(status);
CREATE INDEX idx_flags_flagger ON flags(flagger_id);
CREATE INDEX idx_flags_created_at ON flags(created_at DESC);

-- ============================================================================
-- BADGES & ACHIEVEMENTS
-- ============================================================================

CREATE TYPE badge_type AS ENUM ('Bronze', 'Silver', 'Gold');
CREATE TYPE badge_category AS ENUM ('Participation', 'Expertise', 'Moderation', 'Contribution', 'Special');

CREATE TABLE badge_definitions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) UNIQUE NOT NULL,
    description TEXT NOT NULL,
    badge_type badge_type NOT NULL,
    category badge_category NOT NULL,
    icon_url VARCHAR(500),
    
    -- Criteria (for automation)
    criteria JSONB, -- Flexible criteria definition
    
    is_active BOOLEAN NOT NULL DEFAULT true,
    display_order INTEGER NOT NULL DEFAULT 0,
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE user_badges (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    badge_id UUID NOT NULL REFERENCES badge_definitions(id),
    
    -- Context (e.g., which question/answer earned it)
    earned_for_type VARCHAR(50),
    earned_for_id UUID,
    
    earned_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT unique_user_badge UNIQUE (user_id, badge_id, earned_for_type, earned_for_id)
);

CREATE INDEX idx_user_badges_user ON user_badges(user_id);
CREATE INDEX idx_user_badges_badge ON user_badges(badge_id);
CREATE INDEX idx_user_badges_earned_at ON user_badges(earned_at DESC);

-- ============================================================================
-- REPUTATION HISTORY
-- ============================================================================

CREATE TYPE reputation_action AS ENUM (
    'QuestionUpvoted',           -- +5
    'QuestionDownvoted',         -- -2
    'AnswerUpvoted',             -- +10
    'AnswerDownvoted',           -- -2
    'AnswerAccepted',            -- +15
    'AcceptAnswer',              -- +2
    'EditApproved',              -- +2
    'BountyAwarded',             -- +variable
    'PenaltySpam',               -- -100
    'PenaltyOffensive'           -- -100
);

CREATE TABLE reputation_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    action reputation_action NOT NULL,
    points INTEGER NOT NULL,
    
    -- Context
    related_question_id UUID REFERENCES questions(id),
    related_answer_id UUID REFERENCES answers(id),
    related_user_id UUID REFERENCES users(id), -- e.g., who upvoted
    
    description TEXT,
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_reputation_history_user ON reputation_history(user_id);
CREATE INDEX idx_reputation_history_created_at ON reputation_history(created_at DESC);

-- ============================================================================
-- NOTIFICATIONS
-- ============================================================================

CREATE TYPE notification_type AS ENUM (
    'AnswerReceived',
    'CommentReceived',
    'AnswerAccepted',
    'Upvoted',
    'BadgeEarned',
    'Mentioned',
    'QuestionEdited',
    'AnswerEdited',
    'Flagged'
);

CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    notification_type notification_type NOT NULL,
    
    title VARCHAR(200) NOT NULL,
    message TEXT NOT NULL,
    link_url VARCHAR(500),
    
    -- Context
    related_question_id UUID REFERENCES questions(id),
    related_answer_id UUID REFERENCES answers(id),
    related_user_id UUID REFERENCES users(id), -- Who triggered it
    
    is_read BOOLEAN NOT NULL DEFAULT false,
    read_at TIMESTAMP WITH TIME ZONE,
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_notifications_user ON notifications(user_id, is_read);
CREATE INDEX idx_notifications_created_at ON notifications(created_at DESC);

-- ============================================================================
-- AUDIT LOGS (Critical for enterprise)
-- ============================================================================

CREATE TYPE audit_action AS ENUM (
    'Create', 'Update', 'Delete', 'Restore',
    'Login', 'Logout', 'FailedLogin',
    'RoleChanged', 'Banned', 'Unbanned',
    'QuestionClosed', 'QuestionReopened',
    'FlagHandled', 'UserSuspended'
);

CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Who did it
    user_id UUID REFERENCES users(id),
    username VARCHAR(100),
    ip_address INET,
    user_agent TEXT,
    
    -- What happened
    action audit_action NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    entity_id UUID,
    
    -- Details
    description TEXT,
    old_values JSONB,
    new_values JSONB,
    
    -- Context
    request_id VARCHAR(100), -- For tracing
    session_id VARCHAR(100),
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_audit_logs_user ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX idx_audit_logs_created_at ON audit_logs(created_at DESC);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);
CREATE INDEX idx_audit_logs_ip ON audit_logs(ip_address);

-- ============================================================================
-- ANALYTICS AGGREGATES
-- ============================================================================

CREATE TABLE daily_stats (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    stats_date DATE UNIQUE NOT NULL,
    
    -- Question stats
    questions_asked INTEGER NOT NULL DEFAULT 0,
    questions_answered INTEGER NOT NULL DEFAULT 0,
    answers_accepted INTEGER NOT NULL DEFAULT 0,
    
    -- Engagement
    total_votes INTEGER NOT NULL DEFAULT 0,
    total_comments INTEGER NOT NULL DEFAULT 0,
    total_views INTEGER NOT NULL DEFAULT 0,
    
    -- User stats
    new_users INTEGER NOT NULL DEFAULT 0,
    active_users INTEGER NOT NULL DEFAULT 0,
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_daily_stats_date ON daily_stats(stats_date DESC);

-- ============================================================================
-- ATTACHMENTS
-- ============================================================================

CREATE TYPE attachment_type AS ENUM ('Image', 'Document', 'LogFile', 'Config', 'Other');

CREATE TABLE attachments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Polymorphic relationship
    attached_to_type VARCHAR(50) NOT NULL,
    attached_to_id UUID NOT NULL,
    
    -- File details
    file_name VARCHAR(255) NOT NULL,
    original_file_name VARCHAR(255) NOT NULL,
    file_size BIGINT NOT NULL, -- bytes
    mime_type VARCHAR(100) NOT NULL,
    attachment_type attachment_type NOT NULL,
    
    -- Storage
    storage_path VARCHAR(500) NOT NULL,
    storage_provider VARCHAR(50) NOT NULL DEFAULT 'Local', -- Local, S3, Azure, etc.
    
    -- Metadata
    uploaded_by UUID NOT NULL REFERENCES users(id),
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT valid_file_size CHECK (file_size > 0 AND file_size <= 52428800) -- 50MB max
);

CREATE INDEX idx_attachments_target ON attachments(attached_to_type, attached_to_id);
CREATE INDEX idx_attachments_uploaded_by ON attachments(uploaded_by);

-- ============================================================================
-- SAVED SEARCHES
-- ============================================================================

CREATE TABLE saved_searches (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    name VARCHAR(100) NOT NULL,
    query_params JSONB NOT NULL,
    
    is_notification_enabled BOOLEAN NOT NULL DEFAULT false,
    notification_frequency VARCHAR(20), -- 'Daily', 'Weekly', 'Immediate'
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_saved_searches_user ON saved_searches(user_id);

-- ============================================================================
-- TAG SYNONYMS
-- ============================================================================

CREATE TABLE tag_synonyms (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_tag_id UUID NOT NULL REFERENCES tags(id),
    target_tag_id UUID NOT NULL REFERENCES tags(id),
    created_by UUID NOT NULL REFERENCES users(id),
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT different_tags CHECK (source_tag_id != target_tag_id),
    CONSTRAINT unique_synonym UNIQUE (source_tag_id, target_tag_id)
);

-- ============================================================================
-- TRIGGERS
-- ============================================================================

-- Update updated_at timestamp automatically
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_questions_updated_at BEFORE UPDATE ON questions
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_answers_updated_at BEFORE UPDATE ON answers
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_comments_updated_at BEFORE UPDATE ON comments
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_tags_updated_at BEFORE UPDATE ON tags
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Update question last_activity_at on answer/comment
CREATE OR REPLACE FUNCTION update_question_activity()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        UPDATE questions 
        SET last_activity_at = CURRENT_TIMESTAMP 
        WHERE id = NEW.question_id;
    END IF;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_question_activity_on_answer AFTER INSERT ON answers
    FOR EACH ROW EXECUTE FUNCTION update_question_activity();

-- ============================================================================
-- SEED DATA - Badge Definitions
-- ============================================================================

INSERT INTO badge_definitions (name, description, badge_type, category, criteria) VALUES
-- Participation
('Welcome', 'Complete your user profile', 'Bronze', 'Participation', '{"type": "profile_complete"}'),
('Student', 'Ask your first question', 'Bronze', 'Participation', '{"type": "questions_asked", "count": 1}'),
('Teacher', 'Answer your first question', 'Bronze', 'Participation', '{"type": "answers_given", "count": 1}'),
('Commentator', 'Leave 10 comments', 'Bronze', 'Participation', '{"type": "comments_given", "count": 10}'),
('Supporter', 'Cast 25 upvotes', 'Bronze', 'Participation', '{"type": "upvotes_given", "count": 25}'),

-- Expertise
('Network Guru', 'Earned 100+ reputation in networking topics', 'Silver', 'Expertise', '{"type": "tag_reputation", "tags": ["networking", "firewalls", "switches"], "reputation": 100}'),
('Automation Master', 'Earned 100+ reputation in automation topics', 'Silver', 'Expertise', '{"type": "tag_reputation", "tags": ["scada", "iec-61850", "automation"], "reputation": 100}'),
('Code Wizard', 'Earned 100+ reputation in software topics', 'Silver', 'Expertise', '{"type": "tag_reputation", "tags": ["csharp", "angular", "dotnet"], "reputation": 100}'),
('Hardware Expert', 'Earned 100+ reputation in hardware topics', 'Silver', 'Expertise', '{"type": "tag_reputation", "tags": ["hardware", "firmware", "rugged"], "reputation": 100}'),

-- Contribution
('Nice Answer', 'Answer with score of 10+', 'Bronze', 'Contribution', '{"type": "answer_score", "score": 10}'),
('Good Answer', 'Answer with score of 25+', 'Silver', 'Contribution', '{"type": "answer_score", "score": 25}'),
('Great Answer', 'Answer with score of 50+', 'Gold', 'Contribution', '{"type": "answer_score", "score": 50}'),
('Accepted Answer', 'Get an answer accepted', 'Bronze', 'Contribution', '{"type": "answers_accepted", "count": 1}'),
('Enlightened', 'First answer was accepted with score of 10+', 'Silver', 'Contribution', '{"type": "first_answer_accepted_high_score"}'),

-- Moderation
('Curator', 'Edit 50 posts', 'Bronze', 'Moderation', '{"type": "edits_approved", "count": 50}'),
('Reviewer', 'Review 100 items in review queue', 'Bronze', 'Moderation', '{"type": "reviews_completed", "count": 100}'),
('Deputy', 'Flag 50 posts helpfully', 'Silver', 'Moderation', '{"type": "helpful_flags", "count": 50}'),

-- Special
('Founding Member', 'Among the first 100 users', 'Gold', 'Special', '{"type": "user_rank", "rank": 100}'),
('Department Champion', 'Top contributor in your department', 'Gold', 'Special', '{"type": "department_leader"}');

-- ============================================================================
-- VIEWS FOR COMMON QUERIES
-- ============================================================================

-- Leaderboard view
CREATE OR REPLACE VIEW leaderboard AS
SELECT 
    u.id,
    u.username,
    u.display_name,
    u.department,
    u.reputation,
    u.avatar_url,
    COUNT(DISTINCT q.id) as questions_count,
    COUNT(DISTINCT a.id) as answers_count,
    COUNT(DISTINCT ub.id) as badges_count,
    COUNT(DISTINCT CASE WHEN a.is_accepted THEN a.id END) as accepted_answers_count
FROM users u
LEFT JOIN questions q ON q.author_id = u.id AND q.deleted_at IS NULL
LEFT JOIN answers a ON a.author_id = u.id AND a.deleted_at IS NULL
LEFT JOIN user_badges ub ON ub.user_id = u.id
WHERE u.deleted_at IS NULL AND u.is_active = true
GROUP BY u.id, u.username, u.display_name, u.department, u.reputation, u.avatar_url;

-- Unanswered questions view
CREATE OR REPLACE VIEW unanswered_questions AS
SELECT 
    q.id,
    q.title,
    q.slug,
    q.view_count,
    q.vote_score,
    q.created_at,
    q.last_activity_at,
    u.username as author_username,
    u.display_name as author_display_name,
    ARRAY_AGG(t.name) as tags
FROM questions q
INNER JOIN users u ON q.author_id = u.id
LEFT JOIN question_tags qt ON qt.question_id = q.id
LEFT JOIN tags t ON t.id = qt.tag_id
WHERE q.answer_count = 0 
    AND q.deleted_at IS NULL 
    AND q.status = 'Open'
GROUP BY q.id, q.title, q.slug, q.view_count, q.vote_score, 
         q.created_at, q.last_activity_at, u.username, u.display_name;

-- Hot questions view (activity-based)
CREATE OR REPLACE VIEW hot_questions AS
SELECT 
    q.id,
    q.title,
    q.slug,
    q.view_count,
    q.vote_score,
    q.answer_count,
    q.created_at,
    q.last_activity_at,
    -- Hot score algorithm: weighted combination of votes, answers, views, and recency
    (q.vote_score * 10 + q.answer_count * 5 + q.view_count * 0.1) / 
    (EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - q.created_at)) / 3600 + 2) as hot_score
FROM questions q
WHERE q.deleted_at IS NULL 
    AND q.status = 'Open'
    AND q.created_at > CURRENT_TIMESTAMP - INTERVAL '7 days'
ORDER BY hot_score DESC;

-- ============================================================================
-- FULL TEXT SEARCH CONFIGURATION
-- ============================================================================

-- Create custom text search configuration for technical content
CREATE TEXT SEARCH CONFIGURATION technical_search (COPY = english);

-- Add custom stop words removal for technical terms
ALTER TEXT SEARCH CONFIGURATION technical_search
    DROP MAPPING FOR asciiword;

-- Add support for acronyms and technical terms
ALTER TEXT SEARCH CONFIGURATION technical_search
    ADD MAPPING FOR asciiword WITH english_stem;
