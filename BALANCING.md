# Balancing-Details für Spin a Rayan

## Währungen

**Money (BigInteger)**: Hauptwährung, verdient durch Farming auf Plots. Wird für Rebirths und Shop-Käufe verwendet.

**Gems (int)**: Premium-Währung, verdient durch Quests. Wird für AutoRoll-Freischaltung und Roll-Cooldown-Upgrades verwendet.

## Rebirth-System

Der erste Rebirth kostet 10.000 Money. Jeder weitere Rebirth kostet das Achtfache des vorherigen Preises:
- 1. Rebirth: 10.000
- 2. Rebirth: 80.000
- 3. Rebirth: 640.000
- 4. Rebirth: 5.120.000
- usw.

Mit jedem Rebirth erhält der Spieler:
- +1 Plot-Slot (beginnt mit 3)
- +4 Money-Multiplikator (beginnt mit 1.0, wird zu 1.0 + Rebirths * 4)

## Rayan-Seltenheiten

Rayans werden nach ihrer Seltenheit (1 in X) klassifiziert:
- Common: 1 in 1
- Uncommon: 1 in 5
- Rare: 1 in 20
- Epic: 1 in 100
- Legendary: 1 in 1.000
- Mythic: 1 in 10.000
- Divine: 1 in 100.000
- Celestial: 1 in 1.000.000
- Galactic: 1 in 10.000.000
- Universal: 1 in 100.000.000

Die Seltenheit beeinflusst den Basis-Wert des Rayans exponentiell.

## Suffix-Multiplikatoren

Suffixe sind optional und multiplizieren den Wert eines Rayans:
- Void: 1 in 1.000 Chance, 10x Wert
- Selbstbewusst: 1 in 25 Chance, 1,5x Wert
- GC: 1 in 50 Chance, 2x Wert
- SSL: 1 in 200 Chance, 3x Wert
- Shiny: 1 in 100 Chance, 2,5x Wert
- Golden: 1 in 500 Chance, 5x Wert
- Ancient: 1 in 5.000 Chance, 20x Wert
- Eternal: 1 in 50.000 Chance, 100x Wert

## Farming & Income

Rayans auf Plot-Slots generieren pro Sekunde ihren Gesamtwert als Einkommen. Der Gesamtwert wird durch den Money-Multiplikator des Spielers multipliziert.

Beispiel: Ein Rayan mit Basis-Wert 1.000 und 2x Suffix-Multiplikator hat einen Gesamtwert von 2.000. Mit einem Money-Multiplikator von 5 (nach 1 Rebirth) generiert dieser Rayan 10.000 Money pro Sekunde.

## Shop & Dice

Der Shop wird alle 10 Minuten erneuert. Verfügbare Würfel:
- Common Dice: 1.0 Luck, 100 Money
- Uncommon Dice: 1.5 Luck, 500 Money
- Rare Dice: 2.0 Luck, 2.000 Money
- Epic Dice: 3.0 Luck, 10.000 Money

Die Preise erhöhen sich mit jedem Kauf eines Würfels um Faktor 1,5.

## Roll-Cooldown & AutoRoll

Standard-Cooldown: 2 Sekunden zwischen Rolls.

AutoRoll-Upgrades (mit Gems):
- AutoRoll freischalten: 100 Gems
- Cooldown auf 1 Sekunde: 2.000 Gems
- Cooldown auf 0,5 Sekunden: 5.000 Gems

## Quests

Verfügbare Quests:
- Rolle 100 mal: 100 Gems
- Rolle 1.000 mal: 1.200 Gems
- Spiele 30 Minuten: 2.000 Gems
- Rebirthe 5 mal: 5.000 Gems

Quests werden automatisch verfolgt und Belohnungen können beansprucht werden, wenn die Bedingung erfüllt ist.

## Performance-Optimierungen

- Farming-Berechnung erfolgt einmal pro Sekunde
- Inventar wird nach Wert sortiert
- Datenstrukturen verwenden BindingList für effiziente UI-Updates
- Speichern erfolgt alle 10 Minuten automatisch
