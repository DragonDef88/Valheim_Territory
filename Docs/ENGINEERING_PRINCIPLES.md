# Engineering Principles

## Our Mission

Build a long-term, maintainable territory framework for Valheim.

---

## Principle 1

Architecture before implementation.

---

## Principle 2

Ward is the heart of every Territory.

Without Ward there is no Territory.

---

## Principle 3

The World is the Source of Truth.

The game world defines what exists.

JSON stores only additional data.

---

## Principle 4

Registry is a cache.

Registry can always be rebuilt.

---

## Principle 5

One sprint = one finished feature.

Never leave half-implemented systems.

---

## Principle 6

Small safe iterations.

Large rewrites are avoided.

---

## Principle 7

Documentation is part of the project.

Architecture decisions are documented.

---

## Principle 8

Build for years, not for today.

---

## Principle 9

One module — one question.

Every module should have exactly one responsibility
and answer one architectural question.

If a module starts answering multiple questions,
it should probably be split.

---

##Principle 10

The simplest correct solution wins.

---

##Principle 11

Separate Unity from business logic.

То есть:

Scanner знает Unity.

Discovery знает WardModel.

Territory знает Domain.

Persistence знает JSON.

Каждый слой знает только то, что должен знать.

---

##Principle 12

Every boundary has a translator.

То есть каждый переход между слоями выполняется специальным классом.