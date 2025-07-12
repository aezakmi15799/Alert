local UIManager = {}
UIManager.__index = UIManager

function UIManager:new()
    local self = setmetatable({}, UIManager)
    self.visible = false
    return self
end

function UIManager:show(data)
    self.visible = true
    SetNuiFocus(true, true)
    SendNUIMessage({action = 'show', data = data})
end

function UIManager:update(data)
    SendNUIMessage({action = 'update', data = data})
end

function UIManager:hide()
    self.visible = false
    SetNuiFocus(false, false)
    SendNUIMessage({action = 'hide'})
end

RegisterNUICallback('close', function(_, cb)
    UIManager:hide()
    cb('ok')
end)

return UIManager
