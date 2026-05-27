# STS2 Card Creation
When I run /new-card:
1. Ask for card name, type (Attack/Skill/Power/Event), and rarity
2. grep existing cards of same type for structure reference
3. Generate: Card class file + JSON data + DynamicVar setup
4. Advise correct pool registration (EventCardPool vs CharacterPool vs PowerCardPool)
5. Run build and confirm clean