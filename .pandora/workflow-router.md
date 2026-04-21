# Pandora Workflow Router

## Purpose

This file installs the Pandora-Workflow operating boundary for this repository.

This repo may use Pandora-Workflow guidance for local development, staging records, bounded implementation slices, and review discipline.

## Authority

This file is workflow guidance only.

It does not grant canon authority, runtime authority, autonomous Git authority, local LLM write authority, or downstream feature authority.

## Two-lane workflow

### Lane A: Draft lane

Local/open-source LLMs may produce candidate-only drafts, SEED-style objective logs, implementation-block plans, assumptions, risks, and test ideas.

Draft-lane output is inert.

Draft-lane output must not be treated as implementation, validation, promotion, or repo authority.

### Lane B: Promotion lane

Only reviewed material may be promoted into bounded repository changes.

Promotion requires:

- declared branch
- declared allowed files
- declared required files
- explicit implementation commands
- validation commands
- manual Git review
- manual push / PR review

## Local LLM boundary

Local LLMs, including Qwen-style drafting assistants, may not:

- edit repository files directly
- stage files
- commit
- push
- change branches
- claim validation
- claim implementation
- claim repository state
- decide promotion

## Current adoption status

This is a docs-only workflow adoption scaffold.

No gameplay code, project files, build settings, or runtime behavior are changed by this file.
