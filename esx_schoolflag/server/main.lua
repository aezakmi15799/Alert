ESX = nil
TriggerEvent('esx:getSharedObject', function(obj) ESX = obj end)

local GameManagerClass = require('server/classes/GameManager')
local PlayerClass = require('server/classes/Player')
local TeamClass = require('server/classes/Team')
local TimerClass = require('server/classes/Timer')

GameManager = GameManagerClass:new()

RegisterNetEvent('playerLoaded')
AddEventHandler('playerLoaded', function(playerId, xPlayer)
    GameManager:addPlayer(playerId, {
        identifier = xPlayer.identifier,
        job = xPlayer.getJob().name
    })
end)

RegisterCommand('startflag', function(src, args)
    if src == 0 then
        GameManager:startGame()
    end
end, true)

RegisterNetEvent('schoolflag:pickupFlag')
AddEventHandler('schoolflag:pickupFlag', function(team)
    -- logic to give score
    local player = GameManager.players[source]
    if player and player.job == team then
        local t = GameManager:getTeam(team)
        if t then
            t:addScore(1)
            player:addScore(1)
            GameManager:syncUI()
        end
    end
end)
