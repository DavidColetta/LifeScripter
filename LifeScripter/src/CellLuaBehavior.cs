using MoonSharp.Interpreter;

partial class Cell
{
    private void RegisterCellFunctions() {
        memory["changeGlyph"] = (Action<int>)ChangeGlyph;
        memory["changeColor"] = (Action<int, int, int>)ChangeColor;

        memory["getEnergy"] = (Func<int>)GetEnergy;

        memory["directions"] = new Table(script) {
            ["north"] = Direction.NORTH,
            ["east"] = Direction.EAST,
            ["south"] = Direction.SOUTH,
            ["west"] = Direction.WEST
        };
        memory["look"] = (Func<Direction, Table>)Look;
        memory["move"] = (Func<Direction, bool>)Move;
        memory["eat"] = (Func<Direction, bool>)Eat;
        memory["reproduce"] = (Func<Direction, bool>)Reproduce;
        memory["changeSpeed"] = (Action<int>)ChangeSpeed;
        memory["getMaxEnergy"] = () => MAX_ENERGY;
    }

    public void ChangeGlyph(int glyph) {
        Appearance.Glyph = glyph;
    }

    public void ChangeColor(int r, int g, int b) {
        Appearance.Foreground = new Color(r, g, b);
    }

    public int GetEnergy() {
        return energy;
    }

    public Table Look(Direction direction) {
        Table lookResult = new Table(script);
        lookResult["type"] = "empty";
        Point lookPositioin = Position;
        for (int i = 0; i < SIGHT_DISTANCE; i++) {
            switch (direction) {
                case Direction.NORTH:
                    lookPositioin += (0, -1);
                    break;
                case Direction.EAST:
                    lookPositioin += (1, 0);
                    break;
                case Direction.SOUTH:
                    lookPositioin += (0, 1);
                    break;
                case Direction.WEST:
                    lookPositioin += (-1, 0);
                    break;
                default:
                    throw new ScriptRuntimeException("Invalid direction: " + direction + ". Direction must be an integer from 0 to 3.");
            }
            if (lookPositioin.X < 0 || lookPositioin.X >= World.Width || lookPositioin.Y < 0 || lookPositioin.Y >= World.Height) {
                lookResult["type"] = "wall";
                lookResult["distance"] = i;
                return lookResult;
            }
            WorldObject? entity = World.grid[lookPositioin.X, lookPositioin.Y];
            if (entity != null) {
                if (entity is Food) {
                    lookResult["type"] = "food";
                    lookResult["distance"] = i;
                    return lookResult;
                }
                lookResult["type"] = "entity";
                lookResult["distance"] = i;
                // lookResult["glyph"] = entity.AppearanceSingle?.Appearance.Glyph;
                // lookResult["color"] = entity.AppearanceSingle?.Appearance.Foreground;
                return lookResult;
            }
        }
        return lookResult;
    }

    public bool Move(Direction direction) {
        if (exhausted) {
            return false;
        }
        Point newPosition = Position;
        switch (direction) {
            case Direction.NORTH:
                newPosition += (0, -1);
                break;
            case Direction.EAST:
                newPosition += (1, 0);
                break;
            case Direction.SOUTH:
                newPosition += (0, 1);
                break;
            case Direction.WEST:
                newPosition += (-1, 0);
                break;
            default:
                throw new ScriptRuntimeException("Invalid direction: " + direction + ". Direction must be an integer from 0 to 3.");
        }
        if (!World.IsInBounds(newPosition)) {
            return false;
        }
        if (World.grid[newPosition.X, newPosition.Y] != null) {
            if (!Eat(direction)) {
                return false;
            }
        }
        Reposition(newPosition);
        energy--;
        exhausted = true;
        return true;
    }

    public bool Eat(Direction direction) {
        Point newPosition = Position;
        switch (direction) {
            case Direction.NORTH:
                newPosition += (0, -1);
                break;
            case Direction.EAST:
                newPosition += (1, 0);
                break;
            case Direction.SOUTH:
                newPosition += (0, 1);
                break;
            case Direction.WEST:
                newPosition += (-1, 0);
                break;
            default:
                throw new ScriptRuntimeException("Invalid direction: " + direction + ". Direction must be an integer from 0 to 3.");
        }
        if (!World.IsInBounds(newPosition)) {
            return false;
        }
        if (World.grid[newPosition.X, newPosition.Y] != null) {
            Food? food = World.grid[newPosition.X, newPosition.Y] as Food;
            if (food != null) {//Eat the food
                food.Die();
                energy += food.Nutrition;
                if (energy > MAX_ENERGY) {
                    energy = MAX_ENERGY;
                }
                return true;
            } else {//if there is something else in the way, don't eat
                return false;
            }
        }
        return false;
    }

    public bool Reproduce(Direction direction) {
        if (energy < 2) {
            return false;
        }
        Point newPosition = Position;
        switch (direction) {
            case Direction.NORTH:
                newPosition += (0, -1);
                break;
            case Direction.EAST:
                newPosition += (1, 0);
                break;
            case Direction.SOUTH:
                newPosition += (0, 1);
                break;
            case Direction.WEST:
                newPosition += (-1, 0);
                break;
            default:
                throw new ScriptRuntimeException("Invalid direction: " + direction + ". Direction must be an integer from 0 to 3.");
        }
        if (!World.IsInBounds(newPosition)) {
            return false;
        }
        if (World.grid[newPosition.X, newPosition.Y] != null) {
            return false;
        }
        Cell newCell = new Cell(this, newPosition);
        newCell.energy = energy / 2;
        World.AddSpawnCellEntityToUpdate(newCell);
        energy = energy / 2;
        return true;
    }

    public void ChangeSpeed(int newSpeed) {
        if (TicksPerSecond == newSpeed) {
            return;
        }
        int oldSpeed = TicksPerSecond;
        TicksPerSecond = newSpeed;
        UnsubscribeFromTicks(oldSpeed);
        SubscribeToTicks();
    }

    public enum Direction {
        NORTH = 0,
        EAST = 1,
        SOUTH = 2,
        WEST = 3
    }
}