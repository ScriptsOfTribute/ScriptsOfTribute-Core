#include <iostream>
#include <string>
#include <sstream>
#include <vector>
#include <nlohmann/json.hpp>

using json = nlohmann::json;

std::pair<std::vector<std::string>, int> get_patrons_to_pick() {
    std::string data;
    std::getline(std::cin, data);

    std::istringstream iss(data);
    std::string patrons_str;
    int round_nr;

    iss >> patrons_str >> round_nr;

    std::vector<std::string> patrons;
    std::istringstream patrons_ss(patrons_str);
    std::string patron;

    while (std::getline(patrons_ss, patron, ',')) {
        patrons.push_back(patron);
    }

    return {patrons, round_nr};
}

std::pair<json, bool> get_game_state() {
    std::string data;
    const std::string END_OF_TRANSMISSION = "EOT";
    const std::string FINISHED_TOKEN = "FINISHED";

    while (true) {
        std::string data_fraction;
        std::getline(std::cin, data_fraction);

        if (data_fraction == END_OF_TRANSMISSION) {
            break;
        }

        data += data_fraction;
    }

    if (data.find(FINISHED_TOKEN) == 0) {
        std::istringstream iss(data);
        std::vector<std::string> tokens{std::istream_iterator<std::string>{iss},
                                        std::istream_iterator<std::string>()};

        if (tokens.size() >= 4) {
            json game_over_data;
            game_over_data["winner"] = tokens[1];
            game_over_data["reason"] = tokens[2];
            game_over_data["context"] = tokens[3];

            return {game_over_data, true};
        }
    }

    return {json::parse(data), false};
}