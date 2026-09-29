SELECT 'CREATE DATABASE mdaresna_platform'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'mdaresna_platform')
\gexec

SELECT 'CREATE DATABASE mdaresna_identity'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'mdaresna_identity')
\gexec
