import { PluginCommand } from './plugin-command';
import { CommandArgument } from './command-argument';
import { BuildStepConfig } from './build-step-config';

export interface BuildStep {
  id: number;
  index: number;
  buildStepName: string;
  pluginCommand: PluginCommand;
  pluginCommandId: number;
  workDirectory: string;
  projectId: number;
  commandArguments: CommandArgument[];
  newCommandArgumentKey: string;
  newCommandArgumentValue: string;
  isEditing: boolean;
  dockerImageVersion: string;
  config: string;
  configObject: BuildStepConfig;
}
