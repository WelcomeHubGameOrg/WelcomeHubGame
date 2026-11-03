# Hiking Minigame: writing items and events

Every `*.yaml` file in this folder is read by **Tools → Hiking → Import Definitions** in Unity.
You can split content over as many files as you like (e.g. `events_lakes.yaml`), so people
working in parallel don't edit the same file.

The import checks everything first. If anything is wrong, it lists every error in the Console
and changes nothing. Fix the errors and import again.

Don't edit the generated assets in `Generated/` by hand; the next import overwrites them.

## File layout

```yaml
version: 1      # required, always 1

items:          # optional
  - ...

events:         # optional
  - ...
```

Ids are lowercase letters, digits and `_` (e.g. `rain_jacket`). They must be unique across all
files. **Don't rename an id once it is in the game**; the id is how the game finds the content.

Icons are paths relative to `Assets/Graphics/HikingMinigame/`, e.g. `smallPlants/bush_02.png`.
The image must be imported as a Sprite.

## Items

The player carries up to 3 items. They pick one at the start of the hike and can find more
through events. Items work automatically.

| Field             | Required | Meaning                                                                  |
|-------------------|----------|--------------------------------------------------------------------------|
| `id`              | yes      | Unique id                                                                |
| `name`            | yes      | Name shown to the player                                                 |
| `icon`            | no       | Sprite path                                                              |
| `description`     | no       | Text shown to the player                                                 |
| `protectsAgainst` | no       | List of hazards. Stamina loss from these hazards is blocked completely   |
| `reviveStamina`   | no       | When stamina hits 0, the run continues with this much stamina instead    |
| `singleUse`       | no       | `true`: the item is gone after protecting or reviving once. Default `false` |

Items can also unlock choices (`requiresItem`) and lower risks (`reducedBy`); see below.

## Events

| Field            | Required | Meaning                                                                    |
|------------------|----------|----------------------------------------------------------------------------|
| `id`             | yes      | Unique id                                                                  |
| `name`           | yes      | Title shown in the dialogue                                                |
| `icon`           | no       | Sprite that moves along the road                                           |
| `description`    | no       | Text shown in the dialogue                                                 |
| `weight`         | no       | How often it comes up compared to other events. Default `1`                |
| `maxOccurrences` | no       | How often it can happen per run. Leave out for unlimited                   |
| `trigger`        | no       | `random` (default) or `start`. Exactly one event can be the start event: it opens the run (packing the bag) |
| `choices`        | yes      | At least one choice                                                        |

### Choices

| Field          | Required | Meaning                                                                     |
|----------------|----------|-----------------------------------------------------------------------------|
| `text`         | yes      | Button text                                                                 |
| `requiresItem` | no       | Item id. Without that item the button is greyed out and shows what is needed |
| `outcome`      | yes      | What happens                                                                |
| `risk`         | no       | A chance that something else happens instead of `outcome`                  |

An **outcome** has a `text` (required, shown after the choice) and a list of `effects` (optional).

A **risk** has:

- `chance`: between 0 and 1, e.g. `0.4` = 40 %
- `reducedBy` (optional): item ids, each with how much it lowers the chance, e.g. `first_aid_kit: 0.3`
- `outcome`: what happens if the risk hits. This replaces the normal outcome; it is not added to it

### Effects

Each effect does exactly one thing:

| Effect              | Meaning                                                                     |
|---------------------|-----------------------------------------------------------------------------|
| `stamina: 10`       | Gain or lose stamina. Losses can have a `hazard` that items protect against |
| `nature: -5`        | Change the nature score. It gives points at the end of the run; 0 is not game over |
| `giveItem: <id>`    | Give an item. If the bag is full, the player leaves it behind               |
| `removeItem: <id>`  | Take an item away, e.g. after using it for a `requiresItem` choice          |

Hazards: `rain`, `cold`, `injury`, `sickness`, `insects`, `exhaustion`. Pick the hazard that
describes the real cause, so only items that make sense in real life protect against it
(a rain jacket helps against rain, not against swimming in a cold lake). A stamina loss
without a hazard can't be blocked by any item.

Use `requiresItem` for things an item makes possible (filtering lake water needs a water
filter) and `protectsAgainst` for damage an item prevents. Not every event needs an item.

When stamina reaches 0, an item with `reviveStamina` saves the player. Without one, the run is over.

## Example

```yaml
version: 1

events:
  - id: shortcut
    name: A Shortcut?
    icon: smallPlants/bush_05.png
    description: Maybe if I just cut through the forest here I'm faster...
    weight: 3
    choices:
      - text: Do it
        outcome:
          text: You save a lot of time, but trample some plants.
          effects:
            - stamina: 10
            - nature: -5
        risk:
          chance: 0.4
          outcome:
            text: You trip over a root and twist your ankle.
            effects:
              - stamina: -20
                hazard: injury
      - text: Stay on the path
        outcome:
          text: The marked trail is the safest way.
```
