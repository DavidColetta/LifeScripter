print("Haii!! :3");

changeGlyph(string.byte("0"))
changeColor(0, 0, 0)

moveDir = directions.north;

a = 0;

return function()
    move(moveDir)
    moveDir = (moveDir + 1) % 4
    -- while true do
    --     a = a + 1
    --     if a > 100 then
    --         break
    --     end
    -- end

    print("Ticked")
end