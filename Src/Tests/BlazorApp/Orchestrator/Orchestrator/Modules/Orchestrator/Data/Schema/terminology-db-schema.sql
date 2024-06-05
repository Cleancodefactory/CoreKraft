
CREATE INDEX code_system_fulltext_idx ON code_system USING GIN (to_tsvector('english', embedding_text));