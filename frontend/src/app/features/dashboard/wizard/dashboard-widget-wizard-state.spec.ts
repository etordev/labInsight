import { describe, expect, it } from 'vitest';
import { METRIC_DEFINITION_TECHNICAL_NAMES } from '../models/metric-definition-technical-name';
import { VISUALIZATION_TYPE_TECHNICAL_NAMES } from '../models/visualization-type-technical-name';
import { DashboardWidgetWizardState } from './dashboard-widget-wizard-state';

describe('DashboardWidgetWizardState', () => {
  it('does not allow advancing until a compatible visualization is selected', () => {
    const state = new DashboardWidgetWizardState();
    expect(state.canAdvanceFromContent()).toBe(false);

    state.setMetricDefinition(METRIC_DEFINITION_TECHNICAL_NAMES.DELAYED_ANALYSES);
    expect(state.canAdvanceFromContent()).toBe(false);

    state.setVisualizationType(VISUALIZATION_TYPE_TECHNICAL_NAMES.DATA_GRID);
    expect(state.canAdvanceFromContent()).toBe(true);
  });

  it('clears a visualization that the newly selected metric does not support', () => {
    const state = new DashboardWidgetWizardState();
    state.setMetricDefinition(METRIC_DEFINITION_TECHNICAL_NAMES.DELAYED_ANALYSES);
    state.setVisualizationType(VISUALIZATION_TYPE_TECHNICAL_NAMES.DATA_GRID);

    state.setMetricDefinition(METRIC_DEFINITION_TECHNICAL_NAMES.COMPLETION_RATE);

    expect(state.selectedVisualizationType()).toBeNull();
    expect(state.canAdvanceFromContent()).toBe(false);
  });

  it('keeps a visualization that the newly selected metric still supports', () => {
    const state = new DashboardWidgetWizardState();
    state.setMetricDefinition(METRIC_DEFINITION_TECHNICAL_NAMES.DELAYED_ANALYSES);
    state.setVisualizationType(VISUALIZATION_TYPE_TECHNICAL_NAMES.BAR_CHART);

    state.setMetricDefinition(METRIC_DEFINITION_TECHNICAL_NAMES.ANALYSIS_STATUS);

    expect(state.selectedVisualizationType()).toBe(VISUALIZATION_TYPE_TECHNICAL_NAMES.BAR_CHART);
    expect(state.canAdvanceFromContent()).toBe(true);
  });
});
