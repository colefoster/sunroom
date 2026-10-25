# Product

<!-- impeccable:product-schema 1 -->
<!-- Inferred from BRIEF.md and native/SunglassesApp.swift; the delegating agent instructed "no questions". -->

## Platform

web (marketing page for a native macOS 13+ SwiftUI menu-bar utility)

## Stack

Buildless static HTML/CSS/JS in `dist/` (pinned by brief). No backend, accounts, checkout or outside services.

## Users

A person who runs dark mode everywhere and spends time on Zoom/Teams calls. When the other side shares a light-mode screen, that one region glares while their terminal, browser and other dark apps are fine.

## Product Purpose

Shade one bright region of the screen with a hand-positioned translucent tint rectangle, without touching anything else. Success: the shared screen stops glaring and the person keeps working underneath it.

## Positioning

It is deliberately dumb: a rectangle you place by hand. Not automatic dark-mode conversion, not window tracking, not screen filtering of the whole display.

## Capabilities (source truth)

- Menu-bar item (sun.max.fill), no Dock icon; popover with settings.
- Show/Hide toggle keeps geometry. Move & Resize mode (unlocked) exposes a title bar and resize; Done Moving (locked) ignores mouse events = click-through.
- Color picker; opacity slider 5–80%. Presets: Amber .2/.12/0 @30%, Blue 0/.05/.2 @25%, Gray .05/.05/.05 @35%. Launch tint .15/.1/0 @30%.
- Always-on-top toggle; joins all Spaces; settings and geometry saved locally (AppStorage).
- macOS 13+, SwiftUI. Source: github.com/colefoster/Sunglasses. No packaged download verified; build in Xcode.

## Constraints

No invented pricing, testimonials, health benefits or performance figures.
