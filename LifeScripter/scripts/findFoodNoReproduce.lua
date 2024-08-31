moveDir = directions.west

return function()
    if (math.random() < 0.1) then
        moveDir = (moveDir + 1) % 4
    end

    local closestFoodDist = 1000000
    for i = 0, 3 do
        local lookResult = look(i)
        if (lookResult.type == "food") then
            local dist = lookResult.distance
            if (dist < closestFoodDist) then
                closestFoodDist = dist
                moveDir = i;
            end
        end
    end

    if (not move(moveDir)) then
        moveDir = (moveDir + 1) % 4
    end
end